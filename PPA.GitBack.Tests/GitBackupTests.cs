using System.IO;
using NSubstitute;
using NUnit.Framework;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    class GitBackupTests
    {
        [Test]
        public void PullsFromRepositoryIfItExists()
        {
            // Arrange
            var directory = new DirectoryInfo("directory"); 
            var gitRepository = Substitute.For<IGitRepository>();
            gitRepository.ExistsInDirectory(directory).Returns(true);
            
            var gitBackup = new GitBackup(gitRepository); 

            // Act
            gitBackup.Backup(directory);

            // Assert
            gitRepository.Received().Pull();
        }

        [Test]
        public void ClonesFromRepositoryIfDoesNotExist()
        {
            // Arrange
            var directory = new DirectoryInfo("directory");
            var gitRepository = Substitute.For<IGitRepository>();
            gitRepository.ExistsInDirectory(directory).Returns(false);

            var gitBackup = new GitBackup(gitRepository);

            // Act
            gitBackup.Backup(directory);

            // Assert
            gitRepository.Received().Clone();            
        }
    }
}
