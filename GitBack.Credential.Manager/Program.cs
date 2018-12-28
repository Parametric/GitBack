using CommandLine;
using Ninject;

namespace GitBack.Credential.Manager
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Bootstrapper.ConfigureBindings();
            var kernel = Bootstrapper.Kernel;

            using (var optionsHandler = kernel.Get<OptionsHandler>())
            using (var parser = kernel.Get<Parser>())
            {
                var result = parser.ParseArguments<CredentialHelperOptions>(args)
                                   .MapResult(optionsHandler.HandleOptions, errs => 1);
                return result;
            }
        }
    }

    public class CredentialHelperOptions
    {
        [Value(0, Required = true, HelpText = "List will list the stored Credentials, for the rest See https://git-scm.com/docs/api-credentials#_credential_helpers")]
        public Operation Operation { get; set; }

        [Option('>', "Protocol", HelpText = "The protocol over which the credential will be used (e.g., https).")]
        public string Protocol { get; set; }

        [Option('H', "Host", HelpText = "The remote hostname for a network credential.")]
        public string Host { get; set; }

        [Option("Path", HelpText = "The path with which the credential will be used. E.g., for accessing a remote https repository, this will be the repository’s path on the server.")]
        public string Path { get; set; }

        [Option('U', "Username", HelpText = "The credential’s username, if we already have one (e.g., from a URL, from the user, or from a previously run helper).")]
        public string Username { get; set; }

        [Option('P', "Password", HelpText = "The credential’s password, if we are asking it to be stored.")]
        public string Password { get; set; }

        [Option("Url",
            HelpText = "A convince method that parse that URL and sets the other options. For example -Url https://example.com would set -Protocol to https as well as Host. " +
                       "Note: this options is always applied before the other options when provided on the commandline. " +
                       "See: https://git-scm.com/docs/git-credential#git-credential-codeurlcode")]
        public string Url { get; set; }

        [Option('L', "Listen", Default = YesNo.Default,
            HelpText = "Default: Only listen if Protocol, Host, Path, or Username or a valid URL are not provided. " +
                       "Yes: Always listen, i.e.: Wait for input from Standard in, " +
                       "for what that input looks like See: https://git-scm.com/docs/git-credential#_typical_use_of_git_credential")]
        public YesNo Listen { get; set; }

        [Option('s', "ShowLogger", Default = ShowLoggerOutPut.Error, Separator = ',',
            HelpText = "Displays logger output on the command line via Standard Error")]
        public ShowLoggerOutPut[] LoggerOutput { get; set; }

        public ICredentialRecord ConvertToCredentialRecord()
        {
            var record = new CredentialRecord();

            record.Url = Url;
            record.Host = Host;
            record.Username = Username;
            record.Password = Password;
            record.Path = Path;
            record.Protocol = Protocol;

            return record;
        }
    }

    public enum Operation
    {
        Get,
        Store,
        Erase,
        List
    }

    public enum YesNo
    {
        Default,
        Yes,
        No,
    }

    public enum ShowLoggerOutPut
    {
        None,
        Info,
        Warn,
        Error,
        All,
    }
}

