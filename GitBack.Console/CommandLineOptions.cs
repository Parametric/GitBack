using CommandLine;
using CommandLine.Text;

namespace GitBack.Console
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

        [Option('g', "path to git.exe", Required=true, HelpText = "The full path to the git executable.")]
        public string PathToGit { get; set; }

        [Option('f', "filter projects regex", Required = false, HelpText = "Optional: Filter projects by name with a regular expression pattern")]
        public string ProjectFilter { get; set; }
    }
}