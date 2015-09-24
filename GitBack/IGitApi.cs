using System.Collections.Generic;
using System.IO;

namespace GitBack
{
    public interface IGitApi
    {
        string GetUsername();
        string GetOrganization();
        DirectoryInfo GetBackupLocation();
        string GetPassword();
        IEnumerable<GitRepository> GetRepositories();
       
        void Pull(string repositoryName);
        void Clone(string repositoryName);
    }
}