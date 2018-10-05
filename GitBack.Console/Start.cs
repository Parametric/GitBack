using System.Collections.Generic;
using System.IO;
using CommandLine;
using log4net;
using log4net.Config;
using Ninject;

namespace GitBack.Console
{
    public static class Start
    {
        private static readonly IKernel Kernel = new StandardKernel();

        public static int Main(string[] args)
        {
            XmlConfigurator.Configure();
            Bootstrapper.ConfigureLogging(Kernel);
            Bootstrapper.ConfigureParser(Kernel);

            using (var parser = Kernel.Get<Parser>()) {
                return parser.ParseArguments<CommandLineOptions>(args).MapResult(
                    HandleOptions,
                    HandleParseFailures
                );
        }
    }

        private static int HandleParseFailures(IEnumerable<Error> parseErrors)
        {

            var logger = Kernel.Get<ILog>();

            logger.Error("Parsing Failed");
            foreach (var error in parseErrors)
            {
                logger.Error($"Parsing Error: {error.Tag}. {(error.StopsProcessing ? "Stopped" : "Did not stop")} processing");
            }

            return 1;
        }

        private static int HandleOptions(CommandLineOptions options)
        {
            var programOptions = ConvertCommandLineOptionsToProgramOptions(options);
            Bootstrapper.ConfigureGit(Kernel, programOptions);

            var logger = Kernel.Get<ILog>();
            logger.InfoFormat("Gitback starting...");

            var program = Kernel.Get<Program>();
            program.Execute();

            return 0;
        }

        private static ProgramOptions ConvertCommandLineOptionsToProgramOptions(CommandLineOptions commandLineOptions) => new ProgramOptions
        {
            Username = commandLineOptions.UserName,
            Password = commandLineOptions.Password,
            Organization = commandLineOptions.Organization,
            BackupLocation = new DirectoryInfo(commandLineOptions.BackupLocation),
            PathToGit = commandLineOptions.PathToGit,
            ProjectFilter = commandLineOptions.ProjectFilter
        };
    }
}