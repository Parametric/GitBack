using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var context = new GitContext(new GitApi("username", organization));

            // Act
            var owner = context.GetOwner();

            // Assert
            Assert.That(owner, Is.EqualTo("username"));
        }

        [Test]
        public void Ctor_WithOrganization()
        {
            // Arrange
            var context = new GitContext(new GitApi("username", "organization"));

            // Act
            var owner = context.GetOwner();

            // Assert
            Assert.That(owner, Is.EqualTo("organization"));
        }

        [Test]
        public void GetRepositories()
        {
            // Arrange
            var gitApi = Substitute.For<GitApi>("username", "organization");
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

    }
}
