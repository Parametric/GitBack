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
        private readonly GitClientFactory _clientFactory;
        private readonly ProcessRunner _processRunner;
        private readonly ILog _logger;

        private ProgramOptions ProgramOptions { get; set; }

        public DirectoryInfo BackupLocation => ProgramOptions.BackupLocation;
        public string Username => ProgramOptions.Username;
        public string Organization => ProgramOptions.Organization;
        public string Password => ProgramOptions.Password;

        public GitApi(GitClientFactory clientFactory, ProcessRunner processRunner, ILog logger)
        {
            _clientFactory = clientFactory;
            _processRunner = processRunner;
            _logger = logger;
        }

        public void SetProgramOptions(ProgramOptions programOptions) => ProgramOptions = programOptions;

        public void BackupAllRepos()
        {
            var gitRepositories = GetRepositories();
            var backupDirectory = GetBackupLocation();
            foreach (var gitRepository in gitRepositories)
            {
                _logger.InfoFormat("Backing up {0}.", gitRepository.Name);
                gitRepository.Backup(backupDirectory);
            }
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

                IReadOnlyList<Octokit.Repository> repositories;
                if (String.IsNullOrWhiteSpace(Organization))
                {
                    _logger.Info("Retrieving repositories for current github user.");
                    repositories = repoClient.GetAllForCurrent().Result;
                }
                else
                {
                    _logger.Info(String.Format("Retrieving repositories for: ", Organization));
                    repositories = repoClient.GetAllForOrg(Organization).Result;
                }

                var filter = ProgramOptions.ProjectFilter;

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
            _logger.InfoFormat("Executing Command: {0} {1}", ProgramOptions.PathToGit, argsWithPasswordHidden);

            var startinfo = new ProcessStartInfo
            {
                FileName = ProgramOptions.PathToGit,
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