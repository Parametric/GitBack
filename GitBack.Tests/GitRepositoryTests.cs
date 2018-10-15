using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace GitBack.Tests
{
    [TestFixture]
    class GitRepositoryTests
    {
        [Test]
        public void GetName_ReturnsCorrectName()
        {
            // Arrange
            var api = Substitute.For<IGitApi>();
            const string name = "name";
            var uri = new Uri($"https://example.com/{name}");
            var repoDirectory = new DirectoryInfo($@"C:\temp\{name}");
            var gitRepository = new GitRepository(api, name, uri, repoDirectory);

            // Act
            var result = gitRepository.Name;

            // Assert
            Assert.That(result, Is.EqualTo(name));
        }

        [Test]
        public void Pull_ApiReceivesPullRequest()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            const string name = "repository name";
            var uri = new Uri($"https://example.com/{name}");
            var repoParentDirectory = new DirectoryInfo($@"C:\temp");
            var repoDirectory = new DirectoryInfo(Path.Combine(repoParentDirectory.FullName, name));
            var repository = new GitRepository(gitApi, name, uri, repoParentDirectory, true); 

            // Act
            repository.Pull();

            // Assert
            gitApi.Received().Pull(Arg.Is<DirectoryInfo>(actual => repoDirectory.FullName.Equals(actual.FullName)));
        }

        [Test]
        public void Clone_ApiReceivesCloneRequest()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            const string name = "repository name";
            var uri = new Uri($"https://example.com/{name}");
            var repoDirectory = new DirectoryInfo($@"C:\temp\{name}");
            var repository = new GitRepository(gitApi, name, uri, repoDirectory);

            // Act
            repository.Clone();

            // Assert
            gitApi.Received().Clone(uri, repoDirectory);
        }
    }
}
