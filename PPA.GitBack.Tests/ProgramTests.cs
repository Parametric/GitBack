using NSubstitute;
using NUnit.Framework;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    class ProgramTests
    {
        [Test]
        public void GitContextBacksUpReposWhenExecuteCalled()
        {
            // Arrange
            var gitContext = Substitute.For<GitContext>();
            var program = new Program(gitContext); 
            
            // Act
            program.Execute();

            // Assert
            gitContext.Received().BackupAllRepos();
        }
    }
}
