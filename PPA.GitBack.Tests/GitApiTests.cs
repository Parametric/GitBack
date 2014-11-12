using System.Collections.Generic;
using System.IO;
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

            var clientInitializer = Substitute.For<IGitClientInitializer>();

            var gitApi = new GitApi(programOptions, clientInitializer); 

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
            var clientInitializer = Substitute.For<IGitClientInitializer>();

            var gitApi = new GitApi(programOptions, clientInitializer);

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

            var clientInitializer = Substitute.For<IGitClientInitializer>();

            var gitApi = new GitApi(programOptions, clientInitializer);

            // Act
            var backup = gitApi.GetBackupLocation();
            var result = backup.Equals(backupLocation); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetsRepositoriesFromCorrectAccount()
        {
            // Arrange
            var clientInitializer = Substitute.For<IGitClientInitializer>();
            var repoClient = Substitute.For<IRepositoriesClient>();

            repoClient.GetAllForUser("username").Result.Returns(new List<Repository>()
                {
                    new Repository()
                    {
                        CloneUrl = "url1",
                        Name = "name1"
                    }, 

                    new Repository()
                    {
                        CloneUrl = "url2",
                        Name = "name2"
                    }
                });

            var backupLocation = new DirectoryInfo("backup"); 
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = backupLocation,
                Password = "password"
            };

            clientInitializer.CreateGitClient("username", "password")
                .GetAllForUser("username")
                .Result.Returns(new List<Repository>()
                {
                    new Repository()
                    {
                        CloneUrl = "url1",
                        Name = "name1"
                    }, 

                    new Repository()
                    {
                        CloneUrl = "url2",
                        Name = "name2"
                    }
                });

            var gitApi = new GitApi(programOptions, clientInitializer);

            var expectedRepositories = new List<GitRepository>()
            {
                new GitRepository(gitApi, "url1", backupLocation, "name1"),
                new GitRepository(gitApi, "url2", backupLocation, "name2")
            };


            // Act
            var repositories = gitApi.GetRepositories();

            // Assert
            Assert.That(repositories, Is.EquivalentTo(expectedRepositories));
        }
    }
}
