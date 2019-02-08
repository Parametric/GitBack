using System;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using CommandLine;
using CommandLine.Text;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ExtensionModel.Content;
using ExtendedXmlSerializer.ExtensionModel.Xml;
using log4net.Config;
using Ninject;

namespace GitBack.Credential.Manager
{
    public static class Bootstrapper
    {
        private static IKernel _kernel;
        public static IKernel Kernel {
            get => _kernel ?? (_kernel = new StandardKernel());
            set => _kernel = value;
        }

        public static void ConfigureBindings()
        {
            XmlConfigurator.Configure();

            Kernel.Bind<Parser>().ToMethod(context =>
            {
                return new Parser(s =>
                {
                    s.HelpWriter = null;
                    s.CaseInsensitiveEnumValues = true;
                    s.CaseSensitive = false;
                    s.IgnoreUnknownArguments = true;
                });
            });
            var console = new GitBackStandardConsole();
            var inputOutputManager = new InputOutputManager(true, console);
            console.Logger = inputOutputManager.GetLogger(console.GetType());

            Kernel.Bind<IConsole>().ToConstant(console);
            Kernel.Bind<IInputOutputManager>().ToConstant(inputOutputManager);
            Kernel.Bind<ILoggerFactory>().ToConstant(inputOutputManager);

            Kernel.Bind<IInputOutput>().ToMethod(context => {
                var type = context.Request.ParentRequest == null
                    ? context.Request.Service
                    : context.Request.ParentRequest.Service;

                var manager = context.Kernel.Get<IInputOutputManager>();
                var inputOutput = manager.GetInputOutput(type);

                return inputOutput;
            }).InTransientScope();

            Kernel.Bind<ILogger>().ToMethod(context => {
                var type = context.Request.ParentRequest == null
                    ? context.Request.Service
                    : context.Request.ParentRequest.Service;

                var factory = context.Kernel.Get<ILoggerFactory>();
                var inputOutput = factory.GetLogger(type);

                return inputOutput;
            }).InTransientScope();

            Kernel.Bind<OptionsHandler>().ToSelf().InSingletonScope();

            Kernel.Bind<IExtendedXmlSerializer>().ToMethod(x =>
            {
                var xmlContainer = new ConfigurationContainer()
                                  .UseOptimizedNamespaces()
                                  .UseAutoFormatting();

                return xmlContainer.Create();
            }).InSingletonScope();

            Kernel.Bind<XmlWriterSettings>().ToMethod(x => new XmlWriterSettings
            {
                Indent = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                DoNotEscapeUriAttributes = false,
                WriteEndDocumentOnClose = true,
            });
            Kernel.Bind<XmlReaderSettings>().ToMethod(x => new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true
            });

            Kernel.Bind<IStreamFactory>().To<StreamFactory>();
            Kernel.Bind<IFileStreamer>().To<FileStreamer>();

            var mutexName = typeof(Bootstrapper).Namespace + ".records.lock";
            var mutexTimeout = TimeSpan.FromSeconds(30);
            Kernel.Bind<IMutex>().ToMethod(context => new Mutex(mutexName, mutexTimeout)).InTransientScope();
            Kernel.Bind<IEncryption>().To<LocalUserEncryption>();
            Kernel.Bind<ICredentialRecordsManager>().To<CredentialRecordsManager>();
        }
    }
}