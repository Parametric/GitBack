using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace GitBack.Tests
{
    [TestFixture]
    public class GitContextTests
    {
        [Test]
        public void GetRepositories_ReturnsCorrectRepositores()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            var logger = Substitute.For<ILog>();
            const string name = "name";
            var uri = new Uri($"https://example.com/{name}");
            var repoDirectory = new DirectoryInfo($@"C:\temp\{name}");
            var allRepositories = Builder<GitRepository>
                .CreateListOfSize(10)
                .All()
                .WithFactory(() => new GitRepository(gitApi, name, uri, repoDirectory))
                .Build()
                ;
            var context = new GitContext(gitApi, logger);

            gitApi.GetRepositories().Returns(allRepositories);

            // Act
            var repositories = context.GetRepositories();

            // Assert
            Assert.That(repositories, Is.EquivalentTo(allRepositories));
        }

        [Test]
        public void BackupAllRepos_AllReposCallBackup()
        {
            // Arrange
            var api = Substitute.For<IGitApi>();
            var logger = Substitute.For<ILog>();
            const string name = "name";
            var uri = new Uri($"https://example.com/{name}");
            var parentDirectory = new DirectoryInfo($@"C:\temp");
            var context = new GitContext(api, logger);
            var backupLocation = new DirectoryInfo("backup");

            var allRepositories = new List<GitRepository>
            {
                Substitute.For<GitRepository>(api, name, uri, parentDirectory, true),
                Substitute.For<GitRepository>(api, name, uri, parentDirectory, true),
                Substitute.For<GitRepository>(api, name, uri, parentDirectory, true)
            };

            api.GetRepositories().Returns(allRepositories);

            // Act
            context.BackupAllRepos();

            // Arrange
            foreach (var gitRepository in allRepositories)
            {
               gitRepository.Received().Backup();
            }
        }
    }
}
