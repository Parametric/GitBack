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
            var gitApi = Substitute.For<IGitApi>();
            var allRepositories = Builder<GitRepository>
                .CreateListOfSize(10)
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
        public void CloneNonExistingRepo()
        {
            // Arrange
            const string directory = "directory"; 

            var repository = Substitute.For<GitRepository>();
            //repository.ExistsInDirectory(directory).Returns(false); 

            var gitBackup = Substitute.For<GitBackup>(repository);

            // Act
            repository.Backup(directory);

            // Assert
            gitBackup.Received().Clone(directory); 
        }

        [Test]
        public void PullExistingRepo()
        {
            // Arrange
            const string directory = "directory";

            var repository = Substitute.For<GitRepository>();
            //repository.ExistsInDirectory(directory).Returns(true); 

            var gitBackup = Substitute.For<GitBackup>(repository);

            // Act
            repository.Backup(directory);

            // Assert
            gitBackup.Received().Pull(directory);
        }
    }
}
