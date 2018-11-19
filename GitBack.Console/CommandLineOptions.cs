using CommandLine;
using CommandLine.Text;

namespace GitBack.Console
{
    public class CommandLineOptions
    {
        [Option('u', "username", Required = true, HelpText = "Required: GitHub UserName")]
        public string UserName { get; set; }

        [Option('t', "token",
            HelpText = "Required unless saved: GitHub access token, Generate at https://github.com/settings/tokens, requires 'repo' permissions.")]
        public string Token { get; set; }

        [Option('o', "organization",
            HelpText = "Optional: only repos from the given organization will be backup, otherwise all of them well be. However see ProjectFilter")]
        public string Organization { get; set; }

        [Option('b', "backupLocation", Required = true,
            HelpText = "Required: Location where GitHub repositories are to be backed up.")]
        public string BackupLocation { get; set; }

        [Option('g', "GitPath", Required = true,
            HelpText = "Required: The full path to the git executable.")]
        public string PathToGit { get; set; }

        [Option('f', "ProjectFilter",
            HelpText = "Optional: Filter projects by name with a regular expression pattern.")]
        public string ProjectFilter { get; set; }
    }
}