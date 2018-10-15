using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Octokit;

namespace GitBack.Tests
{
    [TestFixture]
    public class GitClientFactoryTests
    {
        [Test]
        public void CreateEmailsClient()
        {
            // arrange
            const string username = "testUser";
            const string password = "test_password";
            var factory = new GitClientFactory();
            
            // act
            var emailsClient = factory.CreateEmailsClient(username, password);

            // assert
            Assert.That(emailsClient, Is.Not.Null);
        }

        [Test]
        public void CreateGitClient()
        {
            // arrange
            const string username = "testUser";
            const string password = "test_password";
            var factory = new GitClientFactory();
            
            // act
            var gitClient = factory.CreateGitClient(username, password);

            // assert
            Assert.That(gitClient, Is.Not.Null);
        }
    }
}
