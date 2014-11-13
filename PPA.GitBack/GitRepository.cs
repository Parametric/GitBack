using System.IO;

namespace PPA.GitBack
{
    public class GitRepository : IGitRepository
    {
        public string Url { get; set; }
        public string Name { get; set; }
        private readonly IGitApi _gitApi;

        public GitRepository(IGitApi gitApi, string url, string name)
        {
            Url = url;
            _gitApi = gitApi;
            Name = name;
        }

        public string GetName()
        {
            return Name;
        }

        public string GetUrl()
        {
            return Url;
        }

        public void Pull()
        {
            _gitApi.Pull(Url, Name);
            
        }

        public void Clone()
        {
            _gitApi.Clone(Url, Name);
        }

        public bool ExistsInDirectory(DirectoryInfo directory)
        {
            var fullPath = Path.Combine(directory.FullName, Name);
            var repoDirectory = new DirectoryInfo(fullPath);
            return repoDirectory.Exists;
        }

        public void Backup(DirectoryInfo backupDirectory)
        {
            if (ExistsInDirectory(backupDirectory))
            {
                Pull();
            }
            else
            {
                Clone();
            }
        }
    }
}