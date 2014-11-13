using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PPA.GitBack
{
    public class GitApi : IGitApi
    {
        private readonly ProgramOptions _programOptions;
        private readonly IGitClientFactory _clientFactory;
        private readonly IProcessRunner _processRunner;

        public GitApi(ProgramOptions programOptions, IGitClientFactory clientFactory, IProcessRunner processRunner)
        {
            _clientFactory = clientFactory;
            _processRunner = processRunner;
            _programOptions = programOptions;
            UserName = programOptions.Username;
            Organization = programOptions.Organization;
            BackupLocation = programOptions.BackupLocation;
            Password = programOptions.Password;
        }

        public DirectoryInfo BackupLocation { get; private set; }
        public string UserName { get; private set; }
        public string Organization { get; private set; }
        public string Password { get; private set; }

        public IEnumerable<GitRepository> GetRepositories()
        {
            var repoClient = _clientFactory.CreateGitClient(UserName, Password); 

            var repositories = String.IsNullOrWhiteSpace(Organization) 
                ? repoClient.GetAllForUser(UserName).Result 
                : repoClient.GetAllForOrg(Organization).Result;

            return repositories.Select(repository => new GitRepository(this, repository.CloneUrl, BackupLocation, repository.Name));
        }

        public void Pull(string url, DirectoryInfo directory, string name)
        {
            WriteToCmd(url, directory, name, "pull");
        }

        public void Clone(string url, DirectoryInfo directory, string name)
        {
            WriteToCmd(url, directory, name, "clone");
        }

        public string GetUsername()
        {
            return UserName; 
        }

        public string GetOrganization()
        {
            return Organization; 
        }

        public DirectoryInfo GetBackupLocation()
        {
            return BackupLocation; 
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