using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using log4net;
using LibGit2Sharp;
using Octokit;
using Credentials = LibGit2Sharp.Credentials;
using Signature = LibGit2Sharp.Signature;

namespace GitBack
{
    public class GitApi : IGitApi
    {
        private readonly ProgramOptions _programOptions;
        private readonly GitClientFactory _clientFactory;
        private readonly ILocalGitRepositoryHelper _localRepositoryHelper;
        private readonly ILog _logger;

        public DirectoryInfo BackupLocation { get; }
        public string Username { get; }
        public string Organization { get; }
        private string Password { get; }

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

        public IEnumerable<GitRepository> GetRepositories()
        {
            _logger.Info("Retrieving repositories from GitHub.");
            try
            {
                var repoClient = _clientFactory.CreateGitClient(Username, Password);

                IReadOnlyList<Octokit.Repository> repositories;
                if (string.IsNullOrWhiteSpace(Organization))
                {
                    _logger.Info($"Retrieving repositories for current github user: {Username}.");
                    repositories = repoClient.GetAllForCurrent().Result;
                }
                else
                {
                    _logger.Info($"Retrieving repositories for: {Organization}");
                    repositories = repoClient.GetAllForOrg(Organization).Result;
                }

                var filter = _programOptions.ProjectFilter;

                if (!string.IsNullOrEmpty(filter))
                {
                    repositories = repositories.Where(x => Regex.IsMatch(x.Name, filter, RegexOptions.IgnoreCase)).ToList();
                }

                _logger.Info("Repositories retrieved from GitHub.");

                return repositories.Select(repository => 
                    new GitRepository(this, repository.Name, new Uri(repository.CloneUrl), BackupLocation, true));
            }
            catch (AggregateException e)
            {
                _logger.Error(e.Message, e);
                throw;
            }
        }

        public static string GetHostFqdn()
        {
            var domainName = $".{IPGlobalProperties.GetIPGlobalProperties().DomainName}";
            var hostName = Dns.GetHostName();

            if(!hostName.EndsWith(domainName))
            {
                hostName += domainName;
            }

            return hostName;
        }

        private Signature GetSignature()
        {
            var userEmailsClient = _clientFactory.CreateEmailsClient(Username, Password);
            string email = null;
            try
            {
                var emailAddress = userEmailsClient.GetAll().Result.FirstOrDefault(e => e.Primary);
                email = emailAddress?.Email;
            }
            catch (AggregateException e)
            {
                _logger.Info($"Could not retrieve {Username}'s email", e);
            }

            if (email == null)
            {
                email = $"{Username}@{GetHostFqdn()}";
            }

            return new Signature(Username, email, DateTimeOffset.UtcNow);
        }

        public void Pull(DirectoryInfo repositoryLocation)
        {
            var signature = GetSignature();
            var options = new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = CredentialsProvider
                }
            };
            
            _localRepositoryHelper.Pull(repositoryLocation, signature, options);
        }

        public void Clone(Uri gitUrl, DirectoryInfo repositoryLocation)
        {
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