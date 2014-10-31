using System.Collections.Generic;
using System.IO;

namespace PPA.GitBack
{
    public interface IGitApi
    {
        IEnumerable<GitRepository> GetRepositories(string getOwner);

        string GetUsername();
        string GetOrganization();
        void Pull(string url, DirectoryInfo directory);
        void Clone(string url, DirectoryInfo directory);
    }
}