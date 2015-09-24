using System.IO;

namespace GitBack
{
    public class GitRepository
    {
        public string Name { get; set; }
        private readonly IGitApi _gitApi;

        public GitRepository(IGitApi gitApi, string name)
        {
            _gitApi = gitApi;
            Name = name;
        }

        public string GetName()
        {
            return Name;
        }

        public void Pull()
        {
            _gitApi.Pull(Name);
            
        }

        public void Clone()
        {
            _gitApi.Clone(Name);
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