using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PPA.GitBack
{
    public class GitApi : IGitApi
    {
        private readonly ProgramOptions _programOptions;
        private readonly GitClientFactory _clientFactory;
        private readonly ProcessRunner _processRunner;

        public DirectoryInfo BackupLocation { get; private set; }
        public string Username { get; private set; }
        public string Organization { get; private set; }
        public string Password { get; private set; }

        public GitApi(ProgramOptions programOptions, GitClientFactory clientFactory, ProcessRunner processRunner)
        {
            _clientFactory = clientFactory;
            _processRunner = processRunner;
            _programOptions = programOptions;
            Username = programOptions.Username;
            Organization = programOptions.Organization;
            BackupLocation = programOptions.BackupLocation;
            Password = programOptions.Password;
        }

        public string GetUsername()
        {
            return Username;
        }

        public string GetOrganization()
        {
            return Organization;
        }

        public DirectoryInfo GetBackupLocation()
        {
            return BackupLocation;
        }

        public string GetPassword()
        {
            return Password; 
        }

        public IEnumerable<GitRepository> GetRepositories()
        {
             var repoClient = _clientFactory.CreateGitClient(Username, Password); 

            var repositories = String.IsNullOrWhiteSpace(Organization)
                ? repoClient.GetAllForCurrent().Result 
                : repoClient.GetAllForOrg(Organization).Result;

            return repositories.Select(repository => new GitRepository(this, repository.Name));
        }

        public void Pull(string repositoryName)
        {
            WriteToCmd(repositoryName, "pull");
        }

        public void Clone(string repositoryName)
        {
            WriteToCmd(repositoryName, "clone");
        }

        private void WriteToCmd(string repositoryName, string gitCommand)
        {
            var outputDirectory = Path.Combine(BackupLocation.Name, repositoryName);

            var owner = String.IsNullOrWhiteSpace(Organization) ? Username : Organization; 

            var startinfo = new ProcessStartInfo
            {
                FileName = _programOptions.PathToGit,
                Arguments = string.Format("{0} https://{1}:{2}@github.com/{3}/{4}.git {5}", gitCommand, Username, Password, owner, repositoryName, outputDirectory),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            _processRunner.Run(startinfo);
        }
    }
}