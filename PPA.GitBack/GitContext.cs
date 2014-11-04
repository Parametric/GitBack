using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPA.GitBack
{
    public class GitContext
    {
        private readonly IGitApi _gitApi;

        public GitContext(IGitApi gitApi)
        {

            _gitApi = gitApi;

        }

        public string GetOwner()
        {
            var organization = _gitApi.GetOrganization();
            var username = _gitApi.GetUsername();

            return string.IsNullOrWhiteSpace(organization)
                ? username
                : organization;
        }

        public IEnumerable<IGitRepository> GetRepositories()
        {
            return _gitApi.GetRepositories(GetOwner());
        }

        public void BackupAllRepos()
        {
            foreach (var gitBackup in GetRepositories().Select(gitRepository => new GitBackup(gitRepository)))
            {
                gitBackup.Backup(new DirectoryInfo("directory"));
            }
        }
    }
}