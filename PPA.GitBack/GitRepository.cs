using System.IO;

namespace PPA.GitBack
{
    public class GitRepository : IGitRepository
    {
        public string Url { get; set; }
        public DirectoryInfo Directory { get; set; }
        private readonly IGitApi _gitApi;

        public GitRepository(IGitApi gitApi, string url, DirectoryInfo directory)
        {
            Url = url;
            Directory = directory;
            _gitApi = gitApi;
        }

        public void Pull()
        {
            _gitApi.Pull(Url, Directory);
        }

        public void Clone()
        {
            _gitApi.Clone(Url, Directory);
        }

        public bool ExistsInDirectory(DirectoryInfo directory)
        {
            return directory.Exists;
        }    
    }
}