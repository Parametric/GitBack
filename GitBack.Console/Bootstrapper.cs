using System;
using System.IO;
using CommandLine;
using log4net;
using log4net.Config;
using Ninject;

namespace GitBack.Console
{
    internal class Bootstrapper
    {
        public static void ConfigureGit(IKernel kernel, ProgramOptions programOptions)
        {
            kernel.Bind<ProgramOptions>().ToConstant(programOptions);

            kernel.Bind<IGitApi>().To<GitApi>();
            kernel.Bind<IGitContext>().To<GitContext>();
            kernel.Bind<ILocalGitRepositoryHelper>().To<LocalGitRepositoryHelper>();

            XmlConfigurator.Configure();
        }

        public static void ConfigureLogging(IKernel kernel) => kernel.Bind<ILog>().ToMethod(context => {
            var request = context.Request;
            var type = request.ParentRequest == null ? request.Service : request.ParentRequest.Service;

            var log4NetLogger = LogManager.GetLogger(type);
            return log4NetLogger;
        });

        public static void ConfigureParser(IKernel kernel)
        {
            var helpWriter = System.Console.Out;
            kernel.Bind<Parser>().ToMethod(_ => new Parser(settings => settings.HelpWriter = helpWriter));
            kernel.Bind<ParserSettings>().ToMethod(_ => kernel.Get<Parser>().Settings);
        }

    }
}
