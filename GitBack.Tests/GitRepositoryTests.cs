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
            var gitRepository = new GitRepository(api, name);

            // Act
            var result = gitRepository.GetName();

            // Assert
            Assert.That(result, Is.EqualTo(name));
        }

        [Test]
        public void Pull_ApiReceivesPullRequest()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            const string name = "repository name";

            var repository = new GitRepository(gitApi, name); 

            // Act
            repository.Pull();

            // Assert
            gitApi.Received().Pull(name);
        }

        [Test]
        public void Clone_ApiReceivesCloneRequest()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            const string name = "repository name";

            var repository = new GitRepository(gitApi, name);

            // Act
            repository.Clone();

            // Assert
            gitApi.Received().Clone(name);
        }
    }
}
