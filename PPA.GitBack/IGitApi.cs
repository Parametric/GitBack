using System.Collections.Generic;
using System.IO;

namespace PPA.GitBack
{
    public interface IGitApi
    {
        IEnumerable<GitRepository> GetRepositories(string getOwner);

        string GetUsername();
        string GetOrganization();
        void Pull(string url, DirectoryInfo directory, string name);
        void Clone(string url, DirectoryInfo directory, string name);
        DirectoryInfo GetBackupLocation();

    }
}