﻿using NSubstitute;
using NUnit.Framework;

namespace GitBack.Tests
{
    [TestFixture]
    class ProgramTests
    {
        [Test]
        public void Execute_ContextBacksUpAllRepos()
        {
            // Arrange
            var gitContext = Substitute.For<IGitContext>();
            var program = new Program(gitContext); 
            
            // Act
            program.Execute();

            // Assert
            gitContext.Received().BackupAllRepos();
        }
    }
}
