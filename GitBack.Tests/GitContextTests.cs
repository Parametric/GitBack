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
            var allRepositories = Builder<GitRepository>
                .CreateListOfSize(10)
                .All()
                .WithFactory(() => new GitRepository(gitApi, "name"))
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
            var context = new GitContext(api, logger);
            var backupLocation = new DirectoryInfo("backup");

            var allRepositories = new List<GitRepository>
            {
                Substitute.For<GitRepository>(api, name),
                Substitute.For<GitRepository>(api, name),
                Substitute.For<GitRepository>(api, name)
            };

            api.GetRepositories().Returns(allRepositories);
            api.GetBackupLocation().Returns(backupLocation);


            // Act
            context.BackupAllRepos();

            // Arrange
            foreach (var gitRepository in allRepositories)
            {
               gitRepository.Received().Backup(backupLocation);
            }
        }
    }
}
