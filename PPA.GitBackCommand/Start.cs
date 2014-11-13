using System.IO;
using CommandLine;
using CommandLine.Text;
using Ninject;
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
                var kernel = new StandardKernel();

                Bootstrapper.ConfigureNinjectBindings(kernel, programOptions);

                var program = kernel.Get<Program>();
                program.Execute();
            }
        }

        static ProgramOptions ConvertCommandLineOptionsToProgramOptions(CommandLineOptions commandLineOptions)
        {
            return new ProgramOptions
            {
                Username = commandLineOptions.UserName,
                Password = commandLineOptions.Password,
                Organization = commandLineOptions.Organization,
                BackupLocation = new DirectoryInfo(commandLineOptions.BackupLocation),
                PathToGit = commandLineOptions.PathToGit
            };
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
            HelpText = "Input backup location path")]
        public string BackupLocation { get; set; }

        [Option('g', "git.exe location", Required = true,
            HelpText = "Input the location of your Git executable")]
        public string PathToGit { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var application = this.GetType().Assembly;
            var help = new HelpText
            {
                Heading = new HeadingInfo(application.GetName().Name, application.GetName().Version.ToString()),
                AddDashesToOption = true,
                MaximumDisplayWidth = 200
            };

            help.AddOptions(this);

            return help;
        }
    }
}
