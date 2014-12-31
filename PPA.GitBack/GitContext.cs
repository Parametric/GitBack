using System;
using System.Collections.Generic;
using PPA.Logging.Contract;

namespace PPA.GitBack
{
    public class GitContext : IGitContext
    {
        private readonly IGitApi _gitApi;
        private readonly ILogger _logger;

        public GitContext(IGitApi gitApi, ILogger logger)
        {
            _gitApi = gitApi;
            _logger = logger;
        }

        public IEnumerable<GitRepository> GetRepositories()
        {
            return _gitApi.GetRepositories();
        }

        public void BackupAllRepos()
        {            
            var gitRepositories = GetRepositories();
            var backupDirectory = _gitApi.GetBackupLocation();
            foreach (var gitRepository in gitRepositories)
            {
                _logger.InfoFormat("Backing up {0}.", gitRepository.Name);
                gitRepository.Backup(backupDirectory);
            }                
            
        }
    }
}