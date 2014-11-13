using System.Collections.Generic;

namespace PPA.GitBack
{
    public class GitContext : IGitContext
    {
        private readonly IGitApi _gitApi;

        public GitContext(IGitApi gitApi)
        {
            _gitApi = gitApi;
        }

        public IEnumerable<GitRepository> GetRepositories()
        {
            return _gitApi.GetRepositories();
        }

        public void BackupAllRepos()
        {
            var gitRepositories = GetRepositories();
            foreach (var gitRepository in gitRepositories)
            {
                var backupDirectory = _gitApi.GetBackupLocation();
                gitRepository.Backup(backupDirectory);
            }
        }
    }
}