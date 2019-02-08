using System.Collections.Generic;
using CommandLine;

namespace GitBack.Credential.Manager
{
    public class CredentialHelperOptions
    {
        [Value(0, Required = true, MetaName = "operation",
            HelpText = "List will list the stored Credentials, a timestamp of when stored followed by fields identified by the short-form options shown here. "
                     + "For the rest. See https://git-scm.com/docs/api-credentials#_credential_helpers for more information.")]
        public Operation Operation { get; set; }

        [Option('>', "Protocol", HelpText = "The protocol over which the credential will be used (ex:, https).")]
        public string Protocol { get; set; }

        [Option('h', "Host", HelpText = "The remote hostname for a network credential.")]
        public string Host { get; set; }

        [Option('\\', "Path", HelpText = "The path with which the credential will be used. E.g., for accessing a remote https repository, this will be the repository’s path on the server.")]
        public string Path { get; set; }

        [Option('u', "Username", HelpText = "The credential’s username, if we already have one (e.g., from a URL, from the user, or from a previously run helper).")]
        public string Username { get; set; }

        [Option('p', "Password", HelpText = "The credential’s password, if we are asking it to be stored.")]
        public string Password { get; set; }

        [Option(':', "Url",
            HelpText = "A convince method that parse that URL and sets the other options. "
                     + "For example -Url https://example.com would set -Protocol to https as well as Host. " +
                       "Note: this options is always applied before the other options when provided on the commandline. " +
                       "See: https://git-scm.com/docs/git-credential#git-credential-codeurlcode for more information.")]
        public string Url { get; set; }

        [Option('l', "Listen", Default = YesNo.Default,
            HelpText = "By default only listen if Protocol, Host, Path, or Username or a valid URL are NOT provided; " +
                       "or the 'List' Operation IS provided" +
                       "Yes - to always listen, i.e.: Wait for input from Standard in, No to not listen. " +
                       "See: https://git-scm.com/docs/git-credential#_typical_use_of_git_credential for more information.")]
        public YesNo Listen { get; set; }

        [Option('s', "ShowLogger", Separator = ',', 
            HelpText = "Multiple Values may be provided separated by a comma. Displays logger output on the command line via Standard Error. The Default is Error."
                     + "Valid Values: "
                     + nameof(ShowLoggerOutPut.None) + ", "
                     + nameof(ShowLoggerOutPut.Info) + ", "
                     + nameof(ShowLoggerOutPut.Warn) + ", "
                     + nameof(ShowLoggerOutPut.Error) + ", "
                     + nameof(ShowLoggerOutPut.All))]
        public IEnumerable<ShowLoggerOutPut> LoggerOutput { get; set; }

        [Option('r', "ReportLocation",
            HelpText = "Where the Credentials Are stored. "
                     + "This defaults to the Environment Variable: '" + CredentialRecordsManager.RecordEnvironmentVariable + "' if it exists, "
                     + "or '" + CredentialRecordsManager.DefaultRecordDirectoryName + "' (a non-rooted, hidden directory) otherwise. "
                     + "a rooted path starts with a drive letter (for example 'c:\\'), also the shell will usually provide a rooted path for locations starting with '.\\' "
                     + "If the location provided is not rooted, then the user's home directory is prepended to it. "
                     + "If the location is an existing file, then we use it, however, "
                     + "if the location is an existing directory or looks like one, then the filename: '" + CredentialRecordsManager.DefaultRecordFileName + "' "
                     + "is appended."
        )]
        public string ReportLocation { get; set; }
    }
}