using System.IO;

namespace PPA.GitBack
{
    public class GitRepository : IGitRepository
    {
        public string Url { get; set; }
        public DirectoryInfo Directory { get; set; }
        public string Name { get; set; }
        private readonly IGitApi _gitApi;

        public GitRepository(IGitApi gitApi, string url, DirectoryInfo directory, string name)
        {
            Url = url;
            Directory = directory;
            _gitApi = gitApi;
            Name = name;
        }

        public void Pull()
        {
            _gitApi.Pull(Url, Directory, Name);
            
        }

        public void Clone()
        {
            _gitApi.Clone(Url, Directory, Name);
        }

        public bool ExistsInDirectory(DirectoryInfo directory)
        {
            var fullDirectory = Path.Combine(directory.FullName, Name);
            var repoDirectory = new DirectoryInfo(fullDirectory);
            return repoDirectory.Exists;
        }    
    }
}