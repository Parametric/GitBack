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
        public static int Main(string[] args)
        {
            XmlConfigurator.Configure();
            Bootstrapper.ConfigureNinjectBindings();
            var argumentParser = Bootstrapper.Kernel.Get<IArgumentParser>();
            var result = argumentParser.ParseArguments(args);
            return result;
        }
    }

    public interface IArgumentParser
    {
        int ParseArguments(string[] arguments);
    }

    public class ArgumentParser : IArgumentParser
    {
        private readonly ILog _logger;
        private readonly IGitApi _gitApi;

        public ArgumentParser(IGitApi gitApi, ILog logger)
        {
            _logger = logger;
            _gitApi = gitApi;
        }

        public int ParseArguments(string[] arguments)
        {
            using (var parser = new Parser(s => s.HelpWriter = System.Console.Out))
            {
                var result = parser.ParseArguments<CommandLineOptions>(arguments).MapResult(
                    HandleOptions,
                    HandleParseFailures);
                return result;
            }
        }
        private int HandleOptions(CommandLineOptions options)
        {
            _logger.InfoFormat("GitBack starting...");

            var programOptions = ConvertCommandLineOptionsToProgramOptions(options);
            _gitApi.SetProgramOptions(programOptions);
            _gitApi.BackupAllRepos();

            return 0;
        }

        private int HandleParseFailures(IEnumerable<Error> parseErrors)
        {
            _logger.Error("Parsing Failed");
            foreach (var error in parseErrors)
            {
                _logger.Error($"Parsing Error: {error.Tag}. {(error.StopsProcessing ? "Stopped" : "Did not stop")} processing");
            }

            return 1;
        }

        private static ProgramOptions ConvertCommandLineOptionsToProgramOptions(CommandLineOptions commandLineOptions)
        {
            var programOptions = new ProgramOptions()
            {
                Username = commandLineOptions.UserName,
                Token = commandLineOptions.Token,
                Organization = commandLineOptions.Organization,
                BackupLocation = new DirectoryInfo(commandLineOptions.BackupLocation),
                PathToGit = commandLineOptions.PathToGit,
                ProjectFilter = commandLineOptions.ProjectFilter
            };
            return programOptions;
        }
    }
}