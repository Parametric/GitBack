using System.IO;
using CommandLine;
using PPA.GitBack;

namespace PPA.GitBackCommand
{
    public static class Start
    {
        public static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, options))
            {
                var programOptions = ConvertCommandLineOptionsToProgramOptions(options);
                var program = new Program(new ProgramOptions()
                {
                    Username = options.UserName,
                    Password = options.Password,
                    Organization = options.Organization,
                    BackupLocation = new DirectoryInfo(options.BackupLocation)
                });
                program.Execute();
            }
            else
            {
                // TODO: Display Error
            }
        }

        static string[] ConvertCommandLineOptionsToProgramOptions(CommandLineOptions commandLineOptions)
        {
            return null;
        }
    }

    public class CommandLineOptions
    {
        [Option('u', "username", Required = true, 
            HelpText = "Input username")]
        public string UserName { get; set; }

        [Option('p', "password", Required = true, 
            HelpText = "Input password")]
        public string Password { get; set; }

        [Option('o', "organization", Required = false,
            HelpText = "Optional: Input organization")]
        public string Organization { get; set; }

        [Option('b', "backup location", Required = true,
            HelpText = "Input backup location path" )]
        public string BackupLocation { get; set; }
    }
}
