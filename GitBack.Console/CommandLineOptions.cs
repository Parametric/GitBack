using CommandLine;
using CommandLine.Text;

namespace GitBack.Console
{
    [Verb("save", HelpText = "Save GitBack user data")]
    public class SaveOptions : CommandLineOptions
    {
    }

    [Verb("backup", HelpText = "Backup Github Repos")]
    public class BackupOptions : CommandLineOptions
    {
    }

    [Verb("configs", HelpText = "Lookup or Remove GitBack user data")]
    public class ConfigOptions
    {
        [Option('u', "username", Required = false, HelpText = "Username regex to filter the Saved Configs")]
        public string UserNameRegex { get; set; }
        [Option("remove", Required = false,  HelpText = "Removes usernames from config")]
        public bool Remove { get; set; }
        [Option("force", Required = false,  HelpText = "Required to remove more than one user config")]
        public bool Force { get; set; }
    }

    public abstract class CommandLineOptions
    {
        [Option('u', "username", Required = true, 
            HelpText = "Required: Github Username")]
        public string UserName { get; set; }

        [Option('t', "token", Required = false, 
            HelpText = "Github Personal access token. Create one here: https://github.com/settings/tokens. Set Select scopes to repo and user:email.")]
        public string Token { get; set; }

        [Option('o', "organization", Required = false,
            HelpText = "Optional: github organization to backup")]
        public string Organization { get; set; }

        [Option('b', "backup location", Required = false,
            HelpText = "file path to backup github repos")]
        public string BackupLocation { get; set; }

        [Option('f', "filter projects regex", Required = false, HelpText = "Optional: Filter projects by name with a regular expression pattern")]
        public string ProjectFilter { get; set; }
    }
}