using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    class GitRepositoryTests
    {
        [Test]
        public void ApiReceivesRequestWhenPulled()
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
        public void ApiReceivesRequestWhenCloned()
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
