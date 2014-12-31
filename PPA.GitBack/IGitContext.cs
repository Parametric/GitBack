using System.Collections.Generic;

namespace PPA.GitBack
{
    public interface IGitContext
    {
        IEnumerable<GitRepository> GetRepositories();
        void BackupAllRepos();
    }
}
