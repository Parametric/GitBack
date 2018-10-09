using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using LibGit2Sharp;

namespace GitBack
{
    public class GitApi : IGitApi
    {
        private readonly ProgramOptions _programOptions;
        private readonly GitClientFactory _clientFactory;
        private readonly ILocalGitRepositoryHelper _localRepositoryHelper;
        private readonly ILog _logger;

        public DirectoryInfo BackupLocation { get; private set; }
        public string Username { get; private set; }
        public string Organization { get; private set; }
        public string Password { get; private set; }

        public GitApi(ProgramOptions programOptions, GitClientFactory clientFactory, ILocalGitRepositoryHelper localRepositoryHelper, ILog logger)
        {
            _clientFactory = clientFactory;
            _localRepositoryHelper = localRepositoryHelper;
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
            var repositoryPath = Path.Combine(BackupLocation.FullName, repositoryName);
            var repositoryLocation = new DirectoryInfo(repositoryPath);
            var signature = new Signature("name", "email", DateTimeOffset.UtcNow);
            var options = new PullOptions()
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = CredentialsProvider
                }
            };
            
            _localRepositoryHelper.Pull(repositoryLocation, signature, options);
        }

        public void Clone(string repositoryName)
        {
            var owner = string.IsNullOrWhiteSpace(Organization) ? Username : Organization;
            var gitUrl = new Uri($"https://github.com/{owner}/{repositoryName}.git");
            var repositoryPath = Path.Combine(BackupLocation.FullName, repositoryName);
            var repositoryLocation = new DirectoryInfo(repositoryPath);
            
            var options = new CloneOptions
            {
                CredentialsProvider = CredentialsProvider
            };
            _localRepositoryHelper.Clone(gitUrl, repositoryLocation, options);

        }

        private Credentials CredentialsProvider(string url, string username, SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials
            {
                Username = Username,
                Password = Password,
            };
        }
    }

}