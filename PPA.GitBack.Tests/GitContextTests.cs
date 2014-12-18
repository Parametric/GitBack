using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using NSubstitute;
using NUnit.Framework;
using PPA.Logging.Contract;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    public class GitContextTests
    {
        [Test]
        public void GetRepositories_ReturnsCorrectRepositores()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            var logger = Substitute.For<ILogger>();
            var allRepositories = Builder<GitRepository>
                .CreateListOfSize(10)
                .All().WithConstructor(() => new GitRepository(gitApi, "name"))
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
            var logger = Substitute.For<ILogger>();
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
