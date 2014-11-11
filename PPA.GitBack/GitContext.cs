﻿using System.Collections.Generic;

namespace PPA.GitBack
{
    public class GitContext : IGitContext
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
            var owner = GetOwner();
            return _gitApi.GetRepositories(owner);
        }

        public void BackupAllRepos()
        {
            foreach (var gitRepository in GetRepositories())
            {
                var gitBackup = new GitBackup(gitRepository);
                var backupLocation = _gitApi.GetBackupLocation();
                gitBackup.Backup(backupLocation);
            }
        }
    }
}