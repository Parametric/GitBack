using System;
using log4net;
using log4net.Config;
using Ninject;
using PPA.Logging.Contract;

namespace PPA.GitBack.Console
{
    class Bootstrapper
    {
        public static void ConfigureNinjectBindings(IKernel kernel, ProgramOptions programOptions)
        {
            kernel.Bind<ProgramOptions>().ToConstant(programOptions);
            kernel.Bind<ILogger>().ToMethod(context =>
            {
                Type type;
                if (context.Request.ParentRequest == null)
                    type = context.Request.Service;
                else
                    type = context.Request.ParentRequest.Service;

                var log4NetLogger = LogManager.GetLogger(type);
                var logger = new PPA.Logging.Log4Net.Log4NetLogger(log4NetLogger, type);
                return logger;
            });

            kernel.Bind<IGitApi>().To<GitApi>();
            kernel.Bind<IGitContext>().To<GitContext>();

            XmlConfigurator.Configure();
        }
    }
}
