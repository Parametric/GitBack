using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace PPA.GitBack.Tests
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
            var gitRepository = new GitRepository(api, "url", new DirectoryInfo("directory"), name);

            // Act
            var result = gitRepository.GetName();

            // Assert
            Assert.That(result, Is.EqualTo(name));
        }

        [Test]
        public void GetDirectory_ReturnsCorrectDirectory()
        {
            // Arrange
            var api = Substitute.For<IGitApi>();
            var directory = new DirectoryInfo("directory");
            var gitRepository = new GitRepository(api, "url", directory, "name");
        
            // Act
            var result = gitRepository.GetDirectory();

            // Assert
            Assert.That(result, Is.EqualTo(directory));
        }

        [Test]
        public void GetUrl_ReturnsCorrectUrl()
        {
            // Arrange
            var api = Substitute.For<IGitApi>();
            const string url = "url";
            var gitRepository = new GitRepository(api, url, new DirectoryInfo("directory"), "name");

            // Act
            var result = gitRepository.GetUrl();

            // Assert
            Assert.That(result, Is.EqualTo(url));
        }

        [Test]
        public void Pull_ApiReceivesPullRequest()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            var directory = new DirectoryInfo("directory");
            const string name = "repository name";
            const string url = "url";

            var repository = new GitRepository(gitApi, url, directory, name); 

            // Act
            repository.Pull();

            // Assert
            gitApi.Received().Pull(url, directory, name);
        }

        [Test]
        public void Clone_ApiReceivesCloneRequest()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            var directory = new DirectoryInfo("directory");
            const string name = "repository name";
            const string url = "url";

            var repository = new GitRepository(gitApi, url, directory, name);

            // Act
            repository.Clone();

            // Assert
            gitApi.Received().Clone(url, directory, name);
        }
    }
}
