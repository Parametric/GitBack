using System.IO;

namespace PPA.GitBack
{
    public interface IGitRepository
    {
        void Pull();
        void Clone();
        bool ExistsInDirectory(DirectoryInfo directory);
        void Backup(DirectoryInfo backupDirectory);
        string GetName();
        DirectoryInfo GetDirectory();
        string GetUrl();
    }
}