using System;
using System.Collections.Generic;
using System.IO;

namespace GitBack
{
    public interface IGitApi
    {
        string Username { get; }
        string Organization { get; }
        DirectoryInfo BackupLocation { get; }
        IEnumerable<GitRepository> GetRepositories();
       
        void Pull(DirectoryInfo repositoryLocation);
        void Clone(Uri gitUrl, DirectoryInfo repositoryLocation);
    }
}