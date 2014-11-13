using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NSubstitute;
using NUnit.Framework;
using Octokit;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    class GitApiTests
    {
        [Test]
        public void GetUsernameReturnsCorrectUsername()
        {
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = "password"
            };

            var clientInitializer = Substitute.For<IGitClientFactory>();

            var gitApi = new GitApi(programOptions, clientInitializer, null); 

            // Act
            var username = gitApi.GetUsername();
            var result = username.Equals("username"); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetOrganizationReturnsCorrectOrganization()
        {
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = "password"
            };
            var clientInitializer = Substitute.For<IGitClientFactory>();

            var gitApi = new GitApi(programOptions, clientInitializer, null);

            // Act
            var organization = gitApi.GetOrganization();
            var result = organization.Equals("organization");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetBackupLocationReturnsCorrectBackupLocation()
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

            var clientInitializer = Substitute.For<IGitClientFactory>();

            var gitApi = new GitApi(programOptions, clientInitializer, null);

            // Act
            var backup = gitApi.GetBackupLocation();
            var result = backup.Equals(backupLocation); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetRepositories_FromUserAccount_WhenOrganizationIsNotSpecified()
        {
            // Arrange
            var clientInitializer = Substitute.For<IGitClientFactory>();
            const string username = "username";
            const string password = "password";
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer.CreateGitClient(username, password).Returns(repoClient);

            var backupLocation = new DirectoryInfo("backup"); 
            var programOptions = new ProgramOptions()
            {
                Username = username,
                Password = password,
                Organization = null,
                BackupLocation = backupLocation,
            };

            var gitApi = new GitApi(programOptions, clientInitializer, null);

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForUser(username);
        }

        [Test]
        public void GetRepositories_FromOrganization_WhenOrganizationIsSpecified()
        {
            // Arrange
            var clientInitializer = Substitute.For<IGitClientFactory>();
            const string username = "username";
            const string password = "password";
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer.CreateGitClient(username, password).Returns(repoClient);

            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = username,
                Password = password,
                Organization = "organization",
                BackupLocation = backupLocation,
            };

            var gitApi = new GitApi(programOptions, clientInitializer, null);

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForOrg(programOptions.Organization);
        }

        [Test]
        public void GetRepositories_MapsResultsToGitRepositoryObjects()
        {
            // Arrange
            var clientInitializer = Substitute.For<IGitClientFactory>();
            const string username = "username";
            const string password = "password";
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer.CreateGitClient(username, password).Returns(repoClient);

            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = username,
                Password = password,
                Organization = null,
                BackupLocation = backupLocation,
            };

            var allRepositories = Builder<Repository>.CreateListOfSize(2).Build().ToList();
            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            repoClient.GetAllForUser(username).Returns(task);

            var gitApi = new GitApi(programOptions, clientInitializer, null);

            // Act
            var results = gitApi.GetRepositories().ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(allRepositories.Count));
            for (var i = 0; i < results.Count; i++)
            {
                var expected = allRepositories[i];
                var actual = results[i];
                Assert.That(actual.Directory.Name, Is.EqualTo(programOptions.BackupLocation.Name));
                Assert.That(actual.Name, Is.EqualTo(expected.Name));
                Assert.That(actual.Url, Is.EqualTo(expected.CloneUrl));
            }
        }

        [Test]
        public void Pull()
        {
            // Arrange
            var clientInitializer = Substitute.For<IGitClientFactory>();
            var processRunner = Substitute.For<IProcessRunner>();

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = null,
                BackupLocation = new DirectoryInfo("backup"),
                PathToGit = "//some/path/to/git.exe"
            };

            var gitApi = new GitApi(programOptions, clientInitializer, processRunner);

            // Act
            gitApi.Pull("http://some.url.com", programOptions.BackupLocation, "SomeRepo");

            // Assert
            processRunner.Received().Run(Arg.Is<ProcessStartInfo>(arg => IsMatchingProcessStartInfo(arg, programOptions)));
        }

        private static bool IsMatchingProcessStartInfo(ProcessStartInfo arg, ProgramOptions programOptions)
        {
            const string expectedArguments = @"pull http://some.url.com C:\git\GitBack\PPA.GitBack.Tests\bin\Debug\backup\SomeRepo";
            Assert.That(arg.Arguments, Is.EqualTo(expectedArguments), "Arguments");
            Assert.That(arg.WindowStyle, Is.EqualTo(ProcessWindowStyle.Hidden));

            var result = arg.FileName == programOptions.PathToGit
                                              && arg.Arguments == expectedArguments
                                              && arg.WindowStyle == ProcessWindowStyle.Hidden
                                              && arg.CreateNoWindow == true
                                              && arg.RedirectStandardInput == true
                                              && arg.RedirectStandardOutput == true
                                              && arg.UseShellExecute == false;
            return result;
        }
    }
}
