using System.Collections.Generic;
using log4net;


namespace GitBack
{
    public class GitContext : IGitContext
    {
        private readonly IGitApi _gitApi;
        private readonly ILog _logger;

        public GitContext(IGitApi gitApi, ILog logger)
        {
            _gitApi = gitApi;
            _logger = logger;
        }

        public IEnumerable<GitRepository> GetRepositories() => _gitApi.GetRepositories();

        public void BackupAllRepos()
        {            
            var gitRepositories = GetRepositories();
            foreach (var gitRepository in gitRepositories)
            {
                _logger.InfoFormat("Backing up {0}.", gitRepository.Name);
                gitRepository.Backup();
            }                
            
        }
    }
}