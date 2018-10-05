using System;
using log4net;
using log4net.Config;
using Ninject;

namespace GitBack.Console
{
    class Bootstrapper
    {
        public static void ConfigureNinjectBindings(IKernel kernel, ProgramOptions programOptions)
        {
            kernel.Bind<ProgramOptions>().ToConstant(programOptions);
            kernel.Bind<ILog>().ToMethod(context =>
            {
                Type type;
                if (context.Request.ParentRequest == null)
                    type = context.Request.Service;
                else
                    type = context.Request.ParentRequest.Service;

                var log4NetLogger = LogManager.GetLogger(type);
                return log4NetLogger;
            });

            kernel.Bind<IGitApi>().To<GitApi>();
            kernel.Bind<IGitContext>().To<GitContext>();

            XmlConfigurator.Configure();
        }
    }
}
