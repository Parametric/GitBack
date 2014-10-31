using System.IO;

namespace PPA.GitBack
{
    public interface IGitRepository
    {
        void Pull();
        void Clone();
        bool ExistsInDirectory(DirectoryInfo directory);
    }
}