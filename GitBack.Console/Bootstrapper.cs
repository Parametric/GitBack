using System;
using CommandLine;
using log4net;
using log4net.Config;
using Ninject;

namespace GitBack.Console
{
    public static class Bootstrapper
    {
        private static IKernel _kernel;
        public static IKernel Kernel {
            get => _kernel ?? (_kernel = new StandardKernel());
            set => _kernel = value;
        }

        public static void ConfigureNinjectBindings()
        {
            XmlConfigurator.Configure();

            Kernel.Bind<ILog>().ToMethod(context =>
            {
                var type = context.Request.ParentRequest == null
                    ? context.Request.Service
                    : context.Request.ParentRequest.Service;

                var log4NetLogger = LogManager.GetLogger(type);
                return log4NetLogger;
            });

            Kernel.Bind<IGitApi>().To<GitApi>();
            Kernel.Bind<IGitContext>().To<GitContext>();
            Kernel.Bind<IArgumentParser>().To<ArgumentParser>();
            


            
        }
    }
}
