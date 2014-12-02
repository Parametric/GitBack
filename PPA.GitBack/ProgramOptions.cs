using System.IO;
using System.Text.RegularExpressions;

namespace PPA.GitBack
{
    public class ProgramOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Organization { get; set; }
        public DirectoryInfo BackupLocation { get; set; }
        public string PathToGit { get; set; }
        public Regex ProjectFilter { get; set; }
    }
}
