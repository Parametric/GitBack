using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using log4net;
using LibGit2Sharp;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Octokit;
using Repository = Octokit.Repository;
using Signature = Octokit.Signature;

namespace GitBack.Tests
{
    [TestFixture]
    class GitApiTests
    {
        [Test]
        public void GetUsername_ReturnsCorrectUsername()
        { 
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = "password"
            };

            var gitApi = new GitApi(programOptions, null, null, null); 

            // Act
            var username = gitApi.GetUsername();
            var result = username.Equals("username"); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetOrganization_ReturnsCorrectOrganization()
        {
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = "password"
            };

            var gitApi = new GitApi(programOptions, null, null, null);

            // Act
            var organization = gitApi.GetOrganization();
            var result = organization.Equals("organization");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetBackupLocation_ReturnsCorrectBackupLocation()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup"); 

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = backupLocation,
                Password = "password"
            };

           
            var gitApi = new GitApi(programOptions, null, null, null);

            // Act
            var backup = gitApi.GetBackupLocation();
            var result = backup.Equals(backupLocation); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetPassword_ReturnsCorrectPassword()
        {
            // Arrange
            const string password = "password";

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = password
            };


            var gitApi = new GitApi(programOptions, null, null, null);

            // Act
            var backup = gitApi.GetPassword();
            var result = backup.Equals(password);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("        ")]
        public void GetRepositories_FromUserAccount_WhenOrganizationIsNotSpecified(string organization)
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = organization,
                BackupLocation = backupLocation,
            };

            var repoClient = Substitute.For<IRepositoriesClient>();
            var clientInitializer = Substitute.For<GitClientFactory>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var gitApi = new GitApi(programOptions, clientInitializer, Substitute.For<ILocalGitRepositoryHelper>(), Substitute.For<ILog>());

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForCurrent();
        }

        [Test]
        public void GetRepositories_FromOrganization_WhenOrganizationIsSpecified()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = "organization",
                BackupLocation = backupLocation,
            };


            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var gitApi = new GitApi(programOptions, clientInitializer, Substitute.For<ILocalGitRepositoryHelper>(), Substitute.For<ILog>());

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForOrg(programOptions.Organization);
        }

        [Test]
        public void GetRepositories_MapsResultsToGitRepositoryObjects_WithoutFilter()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = null,
                BackupLocation = backupLocation,
            }; 
            
            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var allRepositories = Builder<Repository>.CreateListOfSize(2).Build().ToList();
            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            repoClient.GetAllForCurrent().Returns(task);

            var gitApi = new GitApi(programOptions, clientInitializer, Substitute.For<ILocalGitRepositoryHelper>(), Substitute.For<ILog>());

            // Act
            var results = gitApi.GetRepositories().ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(allRepositories.Count));
            for (var i = 0; i < results.Count; i++)
            {
                var expected = allRepositories[i];
                var actual = results[i];
                Assert.That(actual.GetName(), Is.EqualTo(expected.Name));
            }
        }

        [Test]
        public void GetRepositories_MapsResultsToGitRepositoryObjects_WithFilter()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = null,
                BackupLocation = backupLocation,
                ProjectFilter = "1",
            };

            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var allRepositories = Builder<Repository>.CreateListOfSize(2).Build().ToList();
            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            repoClient.GetAllForCurrent().Returns(task);

            var gitApi = new GitApi(programOptions, clientInitializer, Substitute.For<ILocalGitRepositoryHelper>(), Substitute.For<ILog>());

            // Act
            var results = gitApi.GetRepositories().ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].GetName(), Is.EqualTo(allRepositories[0].Name));
        }

        [Test]
        public void GetRepositories_ThrowsException()
        {
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password"
            };

            var logger = Substitute.For<ILog>();
            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            repoClient.GetAllForCurrent().Throws<AggregateException>();

            clientInitializer.CreateGitClient(programOptions.Username, programOptions.Password).Returns(repoClient);

            var gitApi = new GitApi(programOptions, clientInitializer, Substitute.For<ILocalGitRepositoryHelper>(), logger);

            // Act && Assert 
            Assert.Throws<AggregateException>(() => gitApi.GetRepositories());

        }

        [Test]
        public void Pull()
        {
            // Arrange
            var clientInitializer = Substitute.For<GitClientFactory>();
            var logger = Substitute.For<ILog>();
            var localGitRepositoryHelper = Substitute.For<ILocalGitRepositoryHelper>();

            var backupLocation = Path.Combine(Directory.GetCurrentDirectory(), "backup");
            var programOptions = new ProgramOptions()
            {
                Organization = "organization",
                BackupLocation = new DirectoryInfo(backupLocation)
            };

            const string repositoryName = "SomeRepo";
            var gitApi = new GitApi(programOptions, clientInitializer, localGitRepositoryHelper, logger);

            // Act
            gitApi.Pull(repositoryName);

            // Assert
            var expectedRepositoryLocation = Path.Combine(backupLocation, repositoryName);
            localGitRepositoryHelper.Received().Pull(
                Arg.Is<DirectoryInfo>(d => expectedRepositoryLocation.Equals(d.FullName)), 
                Arg.Any<LibGit2Sharp.Signature>(), 
                Arg.Any<PullOptions>());
        }

        [Test]
        public void Clone()
        {
            // Arrange
            var clientInitializer = Substitute.For<GitClientFactory>();
            var logger = Substitute.For<ILog>();
            var localGitRepositoryHelper = Substitute.For<ILocalGitRepositoryHelper>();

            const string organization = "Organization";
            var backupLocation = Path.Combine(Directory.GetCurrentDirectory(), "backup");
            var programOptions = new ProgramOptions
            {
                Organization = organization,
                BackupLocation = new DirectoryInfo(backupLocation)
            };

            const string repositoryName = "SomeRepo";
            var gitApi = new GitApi(programOptions, clientInitializer, localGitRepositoryHelper, logger);

            // Act
            gitApi.Clone(repositoryName);

            // Assert
            var expectedUri = new Uri($"https://github.com/{organization}/{repositoryName}.git");
            var expectedRepositoryLocation = Path.Combine(backupLocation, repositoryName);
            localGitRepositoryHelper.Received().Clone(
                Arg.Is<Uri>(u => expectedUri.Equals(u)), 
                Arg.Is<DirectoryInfo>(d => expectedRepositoryLocation.Equals(d.FullName)), 
                Arg.Any<CloneOptions>());
        }
    }
}