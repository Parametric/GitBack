using System.IO;
using System.Security.Policy;
using FizzWare.NBuilder;
using NSubstitute;
using NUnit.Framework;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    public class GitContextTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("        ")]
        public void Ctor_WithoutOrganization(string organization)
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            gitApi.GetUsername().Returns("username");
            gitApi.GetOrganization().Returns(organization); 

            var context = new GitContext(gitApi);

            // Act
            var owner = context.GetOwner();

            // Assert
            Assert.That(owner, Is.EqualTo("username"));
        }

        [Test]
        public void Ctor_WithOrganization()
        {
            // Arrange
            var gitApi = Substitute.For<IGitApi>();
            gitApi.GetUsername().Returns("username");
            gitApi.GetOrganization().Returns("organization"); 

            var context = new GitContext(gitApi);

            // Act
            var owner = context.GetOwner();

            // Assert
            Assert.That(owner, Is.EqualTo("organization"));
        }

        [Test]
        public void GetRepositories()
        {
            // Arrange
            var directory = new DirectoryInfo("path");
            var gitApi = Substitute.For<IGitApi>();
            var allRepositories = Builder<GitRepository>
                .CreateListOfSize(10)
                .All().WithConstructor(() => new GitRepository(gitApi, "url", directory, "name"))
                .Build()
                ;
            var context = new GitContext(gitApi);

            gitApi.GetRepositories(context.GetOwner()).Returns(allRepositories);

            // Act
            var repositories = context.GetRepositories();

            // Assert
            Assert.That(repositories, Is.EquivalentTo(allRepositories));
        }

        [Test]
        public void GitApiCallsPullOnExistingRepo()
        {
            // Arrange
            var directory = new DirectoryInfo("path");
            var gitApi = Substitute.For<IGitApi>();
            var repository = new GitRepository(gitApi, "url", directory, "name");

            // Act
            repository.Pull();

            // Assert
            gitApi.Received().Pull("url", directory, "name");
        }

        [Test]
        public void GitApiCallsClonesOnNonExistingRepo()
        {
            // Arrange
            var directory = new DirectoryInfo("path");
            var gitApi = Substitute.For<IGitApi>();
            var repository = new GitRepository(gitApi, "url", directory, "name");

            
            // Act
            repository.Clone();

            // Assert
            gitApi.Received().Clone("url", directory, "name");
        }
    }
}
