using System.Collections.Generic;

namespace GitBack
{
    public interface IGitContext
    {
        IEnumerable<GitRepository> GetRepositories();
        void BackupAllRepos();
    }
}
