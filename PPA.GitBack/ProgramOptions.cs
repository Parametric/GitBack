using System.IO;

namespace PPA.GitBack
{
    public class ProgramOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Organization { get; set; }
        public DirectoryInfo BackupLocation { get; set; }
    }
}
