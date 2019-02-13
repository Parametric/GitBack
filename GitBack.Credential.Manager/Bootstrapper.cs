using System;
using System.Dynamic;
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
using Ninject.Syntax;

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

            Bind<Parser>().ToMethod(context =>
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

            Bind<IConsole>().ToConstant(console);
            Bind<IInputOutputManager>().ToConstant(inputOutputManager);
            Bind<ILoggerFactory>().ToConstant(inputOutputManager);

            Bind<IInputOutput>().ToMethod(context => {
                var type = context.Request.ParentRequest == null
                    ? context.Request.Service
                    : context.Request.ParentRequest.Service;

                var manager = context.Kernel.Get<IInputOutputManager>();
                var inputOutput = manager.GetInputOutput(type);

                return inputOutput;
            }).InTransientScope();

            Bind<ILogger>().ToMethod(context => {
                var type = context.Request.ParentRequest == null
                    ? context.Request.Service
                    : context.Request.ParentRequest.Service;

                var factory = context.Kernel.Get<ILoggerFactory>();
                var inputOutput = factory.GetLogger(type);

                return inputOutput;
            }).InTransientScope();

            Bind<OptionsHandler>().ToSelf().InSingletonScope();

            Bind<IExtendedXmlSerializer>().ToMethod(x =>
            {
                var xmlContainer = new ConfigurationContainer()
                                  .UseOptimizedNamespaces()
                                  .UseAutoFormatting();

                return xmlContainer.Create();
            }).InSingletonScope();

            Bind<XmlWriterSettings>().ToMethod(x => new XmlWriterSettings
            {
                Indent = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                DoNotEscapeUriAttributes = false,
                WriteEndDocumentOnClose = true,
            });
            Bind<XmlReaderSettings>().ToMethod(x => new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true
            });

            Bind<IStreamFactory>().To<StreamFactory>();
            Bind<IFileStreamer>().To<FileStreamer>();

            //Kernel.Bind<IMutex>().ToMethod(context => new Mutex(mutexName, mutexTimeout)).InTransientScope();
            Bind<IMutexFactory>().To<MutexFactory>().InSingletonScope();
            Bind<IEncryption>().To<LocalUserEncryption>();
            Bind<ICredentialRecordsManager>().To<CredentialRecordsManager>();
        }

        public static IBindingToSyntax<T> Bind<T>()
        {
            if (Kernel.CanResolve<T>())
            {
                throw new InvalidOperationException($"{typeof(T)} is already Bound");
            }

            return Kernel.Bind<T>();
        }
    }
}