using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace GitBack
{
    public class GitApi : IGitApi
    {
        private readonly ProgramOptions _programOptions;
        private readonly GitClientFactory _clientFactory;
        private readonly ProcessRunner _processRunner;
        private readonly ILog _logger;

        public DirectoryInfo BackupLocation { get; private set; }
        public string Username { get; private set; }
        public string Organization { get; private set; }
        public string Password { get; private set; }

        public GitApi(ProgramOptions programOptions, GitClientFactory clientFactory, ProcessRunner processRunner, ILog logger)
        {
            _clientFactory = clientFactory;
            _processRunner = processRunner;
            _logger = logger;
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
            _logger.Info("Retrieving repositories from GitHub.");
            try
            {
                var repoClient = _clientFactory.CreateGitClient(Username, Password);

                var repositories = String.IsNullOrWhiteSpace(Organization)
                    ? repoClient.GetAllForCurrent().Result
                    : repoClient.GetAllForOrg(Organization).Result;

                var filter = _programOptions.ProjectFilter;

                if (!String.IsNullOrEmpty(filter))
                {
                    repositories = repositories.Where(x => Regex.IsMatch(x.Name, filter, RegexOptions.IgnoreCase)).ToList();
                }

                _logger.Info("Repositories retrieved from GitHub.");

                return repositories.Select(repository => new GitRepository(this, repository.Name));
            }
            catch (System.AggregateException e)
            {
                _logger.Error(e.Message, e);
                throw;
            }
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
            var outputDirectory = Path.Combine(BackupLocation.FullName, repositoryName);

            var owner = String.IsNullOrWhiteSpace(Organization) ? Username : Organization;

            var args = "";

            var giturl = string.Format("https://{0}:{1}@github.com/{2}/{3}.git", Username, Password, owner, repositoryName);

            switch (gitCommand.ToLower())
            {
                case "pull":
                    args = string.Format("-C {0} {1} {2}", outputDirectory, gitCommand, giturl);
                    break;
                case "clone":
                    args = string.Format("{0} {1} {2}", gitCommand, giturl, outputDirectory);
                    break;
            }


            var argsWithPasswordHidden = giturl.Replace(Password, "********");
            _logger.InfoFormat("Executing Command: {0} {1}", _programOptions.PathToGit, argsWithPasswordHidden);

            var startinfo = new ProcessStartInfo
            {
                FileName = _programOptions.PathToGit,
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            _processRunner.Run(startinfo);
        }
    }

}