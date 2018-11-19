using System.IO;

namespace GitBack
{
    public class ProgramOptions
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string Organization { get; set; }
        public DirectoryInfo BackupLocation { get; set; }
        public string PathToGit { get; set; }
        public string ProjectFilter { get; set; }
    }
}
