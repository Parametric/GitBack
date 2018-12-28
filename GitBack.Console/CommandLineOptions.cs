using CommandLine;
using CommandLine.Text;

namespace GitBack.Console
{
    public class CommandLineOptions
    {
        [Option('u', "username", HelpText = "GitHub UserName, only Required if not using token. Note: if you saved a username / password combination, then username is required")]
        public string UserName { get; set; }

        [Option('p', "password", HelpText = "GitHub Password, only Required if not saved, not using token, and username is also provided. Using token is strongly advised.")]
        public string Password { get; set; }

        [Option('t', "token",
            HelpText = "Required unless saved: GitHub access token,  only Required if not saved, and not using username / password. Generate at https://github.com/settings/tokens, requires 'repo' permissions.")]
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


        private const string CredentialActionHelpText =
            "Optional: Defaults to 'Fail'. " +
            "Indicates how the Token or Username / password will be persisted, and is only meaningful if a token or username are provided. " +
            "'Fail', 'Override', 'UseSaved', and 'OnlySave' all attempt to save the password, but differ if a saved credential is already found. " +
            "'UseCache' will only persist the credentals in memory, and is only useful for testing or if 'credential-manager' or 'credtial-wincred' are not installed.";

       [Option('c', "CredentialAction", HelpText = CredentialActionHelpText)]
        public CredentialAction CredentialAction { get; set; } = CredentialAction.Fail;
    }

    public enum CredentialAction
    {
        Fail,
        Override,
        UseSaved,
        UseCache,
        OnlySave
    }
}