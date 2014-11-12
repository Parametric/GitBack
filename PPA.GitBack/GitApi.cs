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
        private readonly IGitClientInitializer _clientInitializer;

        public GitApi(ProgramOptions programOptions, IGitClientInitializer clientInitializer)
        {
            _clientInitializer = clientInitializer;
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
            var repoClient = _clientInitializer.CreateGitClient(UserName, Password); 

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

            var cmdprocess = new Process();
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

            cmdprocess.StartInfo = startinfo;
            cmdprocess.Start();
        }
    }
}