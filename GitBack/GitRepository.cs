using System;
using System.IO;

namespace GitBack
{
    public class GitRepository
    {
        public string Name { get; }
        public Uri Url { get; }
        public DirectoryInfo ParentDirectory { get; }
        public DirectoryInfo GitDirectory { get; }
        private readonly IGitApi _gitApi;

        public GitRepository(IGitApi gitApi, string name, Uri url, DirectoryInfo directory, bool isParentDirectory=false)
            : this(gitApi, name, url, 
                isParentDirectory? directory : directory.Parent,
                isParentDirectory? new DirectoryInfo(Path.Combine(directory.FullName, name)) : directory)
        {
        }

        public GitRepository(IGitApi gitApi, string name, Uri url, DirectoryInfo parentDirectory, DirectoryInfo gitDirectory)
        {
            _gitApi = gitApi;
            Name = name;
            Url = url;
            ParentDirectory = parentDirectory;
            GitDirectory = gitDirectory;
        }

        public void Pull()
        {
            _gitApi.Pull(GitDirectory);
        }

        public void Clone()
        {
            _gitApi.Clone(Url, GitDirectory);
        }

        public bool ExistsInDirectory()
        {
            if (!ParentDirectory.Exists)
            {
                ParentDirectory.Create();
            }

            return GitDirectory.Exists;
        }
        // fix this to use GitDirectory
        public void Backup()
        {
            if (ExistsInDirectory())
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