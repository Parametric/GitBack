using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Octokit;
using Octokit.Internal;

namespace PPA.GitBack
{
    public class GitApi : IGitApi
    {
        public GitApi(ProgramOptions programOptions)
        {
            UserName = programOptions.Username;
            Organization = programOptions.Organization;
            BackupLocation = programOptions.BackupLocation;
            Password = programOptions.Password;
        }

        public DirectoryInfo BackupLocation { get; private set; }
        public string UserName { get; private set; }
        public string Organization { get; private set; }
        public string Password { get; private set; }

        public IEnumerable<GitRepository> GetRepositories(string owner)
        {
            var connection = new Connection(new ProductHeaderValue("GitBack"),
                new InMemoryCredentialStore(new Credentials(UserName, Password)));

            var apiConnection = new ApiConnection(connection);

            var repoClient = new RepositoriesClient(apiConnection);

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

        private static void WriteToCmd(string url, DirectoryInfo directory, string name, string gitCommand)
        {
            Console.WriteLine("current process: " + gitCommand);
            var fullDirectory = directory + "\\" + name;

            var cmdprocess = new Process();
            var startinfo = new ProcessStartInfo
            {
                FileName = @"C:\\Program Files (x86)\\Git\\cmd\\git.exe",
                Arguments = gitCommand + " " + url + " " + fullDirectory,
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