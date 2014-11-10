using CommandLine;
using CommandLine.Text;

namespace PPA.GitBackCommand
{
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