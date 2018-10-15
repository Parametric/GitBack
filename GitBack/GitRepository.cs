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
            _gitApi = gitApi ?? throw new ArgumentNullException(nameof(gitApi));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            ParentDirectory = parentDirectory;
            GitDirectory = gitDirectory ?? throw new ArgumentNullException(nameof(gitDirectory));
        }

        public void Pull() => _gitApi?.Pull(GitDirectory);

        public void Clone() => _gitApi?.Clone(Url, GitDirectory);

        public bool ExistsInDirectory()
        {
            if (!ParentDirectory.Exists)
            {
                ParentDirectory.Create();
            }

            return GitDirectory.Exists;
        }
        
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