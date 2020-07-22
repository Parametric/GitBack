using System;
using System.IO;
using LibGit2Sharp;

namespace GitBack
{
    public interface ILocalGitRepositoryHelper
    {
        Repository Clone(Uri sourceUrl, DirectoryInfo repositoryLocation, CloneOptions options);
        MergeResult Pull(DirectoryInfo repositoryLocation, Signature merger, PullOptions options);
    }
}