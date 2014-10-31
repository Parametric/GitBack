using System.IO;

namespace PPA.GitBack
{
    public interface IGitBackup
    {
        void Backup(DirectoryInfo directory);
    }
}