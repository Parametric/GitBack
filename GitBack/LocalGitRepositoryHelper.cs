using System;
using System.IO;
using LibGit2Sharp;

namespace GitBack
{
    public class LocalGitRepositoryHelper : ILocalGitRepositoryHelper
    {
        public Repository Clone(Uri sourceUrl, DirectoryInfo repositoryLocation, CloneOptions options)
        {
            var repositoryPath = repositoryLocation.FullName;
            var clonedPath = Repository.Clone(sourceUrl.AbsoluteUri, repositoryPath, options);
            return new Repository(clonedPath);
        }

        public MergeResult Pull(DirectoryInfo repositoryLocation, Signature merger, PullOptions options)
        {
            var repositoryPath = repositoryLocation.FullName;
            var repository = new Repository(repositoryPath);
            return Commands.Pull(repository, merger, options);
        }
    }
}