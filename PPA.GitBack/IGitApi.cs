using System.Collections.Generic;
using System.IO;

namespace PPA.GitBack
{
    public interface IGitApi
    {
        string GetUsername();
        string GetOrganization();
        DirectoryInfo GetBackupLocation();
        IEnumerable<GitRepository> GetRepositories();

        void Pull(string url, string name);
        void Clone(string url, string name);
    }
}