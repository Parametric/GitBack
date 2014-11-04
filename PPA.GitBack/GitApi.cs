using System.Collections.Generic;
using System.IO;

namespace PPA.GitBack
{
    public class GitApi : IGitApi
    {
        public GitApi(ProgramOptions programOptions)
        {
            UserName = programOptions.Username;
            Organization = programOptions.Organization;
            BackupLocation = programOptions.BackupLocation;

        }

        public DirectoryInfo BackupLocation { get; private set; }
        public string UserName { get; private set; }
        public string Organization { get; private set; }

        public IEnumerable<GitRepository> GetRepositories(string getOwner)
        {
            yield break;
        }

        public void Pull(string url, DirectoryInfo directory)
        {
            
        }

        public void Clone(string url, DirectoryInfo directory)
        {
            
        }

        public string GetUsername()
        {
            return UserName; 
        }

        public string GetOrganization()
        {
            return Organization; 
        }

        public DirectoryInfo GetBackupLocation()
        {
            return BackupLocation; 
        }
    }
}