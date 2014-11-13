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
        private readonly IGitClientFactory _clientFactory;
        private readonly IProcessRunner _processRunner;

        public DirectoryInfo BackupLocation { get; private set; }
        public string Username { get; private set; }
        public string Organization { get; private set; }
        public string Password { get; private set; }

        public GitApi(ProgramOptions programOptions, IGitClientFactory clientFactory, IProcessRunner processRunner)
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

        public IEnumerable<IGitRepository> GetRepositories()
        {
            var repoClient = _clientFactory.CreateGitClient(Username, Password); 

            var repositories = String.IsNullOrWhiteSpace(Organization) 
                ? repoClient.GetAllForUser(Username).Result 
                : repoClient.GetAllForOrg(Organization).Result;

            return repositories.Select(repository => new GitRepository(this, repository.CloneUrl, repository.Name));
        }

        public void Pull(string url, string name)
        {
            WriteToCmd(url, BackupLocation, name, "pull");
        }

        public void Clone(string url, string name)
        {
            WriteToCmd(url, BackupLocation, name, "clone");
        }

        private void WriteToCmd(string url, DirectoryInfo directory, string repositoryName, string gitCommand)
        {
            var outputDirectory = Path.Combine(directory.FullName, repositoryName);

            var startinfo = new ProcessStartInfo
            {
                FileName = _programOptions.PathToGit,
                Arguments = string.Format("{0} {1} {2}", gitCommand, url, outputDirectory),
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