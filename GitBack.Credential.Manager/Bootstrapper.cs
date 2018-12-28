using CommandLine;
using log4net.Config;
using Ninject;

namespace GitBack.Credential.Manager {
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
                    s.HelpWriter = System.Console.Out;
                    s.CaseInsensitiveEnumValues = true;
                    s.CaseSensitive = false;
                    s.IgnoreUnknownArguments = true;
                });
            });

            Kernel.Bind<IInputOutputManager>().ToConstant(new InputOutputManager(true));
            
            Kernel.Bind<IInputOutput>().ToMethod(context =>
            {
                var type = context.Request.ParentRequest == null
                    ? context.Request.Service
                    : context.Request.ParentRequest.Service;

                var inputOutputManager = context.Kernel.Get<IInputOutputManager>();
                var inputOutput = inputOutputManager.GetInputOutput(type);

                return inputOutput;
            });

            Kernel.Bind<OptionsHandler>().ToSelf().InSingletonScope();
            Kernel.Bind<IMutex>().To<Mutex>();
        }
    }
}