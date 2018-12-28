using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using log4net;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Octokit;

namespace GitBack.Tests
{
    [TestFixture]
    class GitApiTests
    {
        [Test]
        public void GetUsername_ReturnsCorrectUsername()
        { 
            // Arrange
            const string expectedUsername = "username";
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Username = expectedUsername)
                .Build();

            var gitApi = new GitApi(null, null, null);
            gitApi.SetProgramOptions(programOptions);

            // Act
            var username = gitApi.GetUsername();
            var result = username.Equals(expectedUsername);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetOrganization_ReturnsCorrectOrganization()
        {
            // Arrange
            const string expectedOrganization = "organization";
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Organization = expectedOrganization)
                .Build();

            var gitApi = new GitApi(null, null, null);
            gitApi.SetProgramOptions(programOptions);

            // Act
            var organization = gitApi.GetOrganization();
            var result = organization.Equals(expectedOrganization);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetBackupLocation_ReturnsCorrectBackupLocation()
        {
            // Arrange
            var expectedBackupLocation = new DirectoryInfo("backup");
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.BackupLocation = expectedBackupLocation)
                .Build();
           
            var gitApi = new GitApi(null, null, null);
            gitApi.SetProgramOptions(programOptions);

            // Act
            var backup = gitApi.GetBackupLocation();
            var result = backup.Equals(expectedBackupLocation);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetPassword_ReturnsCorrectPassword()
        {
            // Arrange
            const string expectedToken = "token";
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Token = expectedToken)
                .Build();

            var gitApi = new GitApi(null, null, null);
            gitApi.SetProgramOptions(programOptions);

            // Act
            var backup = gitApi.GetPassword();
            var result = backup.Equals(expectedToken);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("        ")]
        public void GetRepositories_FromUserAccount_WhenOrganizationIsNotSpecified(string organization)
        {
            // Arrange
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Organization = null)
                .Build();

            var repoClient = Substitute.For<IRepositoriesClient>();
            var clientInitializer = Substitute.For<GitClientFactory>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Token)
                .Returns(repoClient)
                ;

            var gitApi = new GitApi(clientInitializer, null, Substitute.For<ILog>());
            gitApi.SetProgramOptions(programOptions);

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForCurrent();
        }

        [Test]
        public void GetRepositories_FromOrganization_WhenOrganizationIsSpecified()
        {
            // Arrange
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .Build();

            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Token)
                .Returns(repoClient)
                ;

            var gitApi = new GitApi(clientInitializer, null, Substitute.For<ILog>());
            gitApi.SetProgramOptions(programOptions);

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForOrg(programOptions.Organization);
        }

        [Test]
        public void GetRepositories_MapsResultsToGitRepositoryObjects_WithoutFilter()
        {
            // Arrange
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Organization = null)
                .With(x => x.ProjectFilter = null)
                .Build();
            
            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Token)
                .Returns(repoClient)
                ;

            var allRepositories = Builder<Repository>.CreateListOfSize(2).Build().ToList();
            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            repoClient.GetAllForCurrent().Returns(task);

            var gitApi = new GitApi(clientInitializer, null, Substitute.For<ILog>());
            gitApi.SetProgramOptions(programOptions);

            // Act
            var results = gitApi.GetRepositories().ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(allRepositories.Count));
            for (var i = 0; i < results.Count; i++)
            {
                var expected = allRepositories[i];
                var actual = results[i];
                Assert.That(actual.GetName(), Is.EqualTo(expected.Name));
            }
        }

        [Test]
        public void GetRepositories_MapsResultsToGitRepositoryObjects_WithFilter()
        {
            // Arrange
            var allRepositories = Builder<Repository>.CreateListOfSize(2).Build().ToList();
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Organization = "")
                .With(x => x.ProjectFilter = allRepositories[0].Name)
                .Build();

            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Token)
                .Returns(repoClient)
                ;

            
            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            repoClient.GetAllForCurrent().Returns(task);

            var gitApi = new GitApi(clientInitializer, null, Substitute.For<ILog>());
            gitApi.SetProgramOptions(programOptions);

            // Act
            var results = gitApi.GetRepositories().ToList();

            // Assert
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].GetName(), Is.EqualTo(allRepositories[0].Name));
        }

        [Test]
        public void GetRepositories_ThrowsException()
        {
            // Arrange
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.Organization = "")
                .Build();

            var logger = Substitute.For<ILog>();
            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            repoClient.GetAllForCurrent().Throws<AggregateException>();

            clientInitializer.CreateGitClient(programOptions.Username, programOptions.Token).Returns(repoClient);

            var gitApi = new GitApi(clientInitializer, null, logger);
            gitApi.SetProgramOptions(programOptions);

            // Act && Assert 
            Assert.Throws<AggregateException>(() => gitApi.GetRepositories());

        }

        [Test]
        public void Pull()
        {
            // Arrange
            var clientInitializer = Substitute.For<GitClientFactory>();
            var logger = Substitute.For<ILog>();
            var processRunner = Substitute.For<ProcessRunner>(logger);

            var backupLocation = new DirectoryInfo("backup");
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.BackupLocation = backupLocation)
                .Build();

            var gitApi = new GitApi(clientInitializer, processRunner, logger);
            gitApi.SetProgramOptions(programOptions);

            // Act
            gitApi.Pull("SomeRepo");

            // Assert
            processRunner.Received().Run(Arg.Is<ProcessStartInfo>(arg => IsMatchingProcessStartInfo(arg, programOptions, "pull")));
        }

        [Test]
        public void Clone()
        {
            // Arrange
            var clientInitializer = Substitute.For<GitClientFactory>();
            var logger = Substitute.For<ILog>();
            var processRunner = Substitute.For<ProcessRunner>(logger);

            var backupLocation = new DirectoryInfo("backup");
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.BackupLocation = backupLocation)
                .Build();

            var gitApi = new GitApi(clientInitializer, processRunner, logger);
            gitApi.SetProgramOptions(programOptions);

            // Act
            gitApi.Clone("SomeRepo");

            // Assert
            processRunner.Received().Run(Arg.Is<ProcessStartInfo>(arg => IsMatchingProcessStartInfo(arg, programOptions, "clone")));
        }

        private static bool IsMatchingProcessStartInfo(ProcessStartInfo arg, ProgramOptions programOptions, string gitCommand)
        {
            var expectedArguments = "";
            const string https = "https";
            var username = programOptions.Username;
            var token = programOptions.Token;
            var organization = programOptions.Organization;
            switch (gitCommand.ToLower())
            {
                case "pull":
                    expectedArguments =$@"-C {Directory.GetCurrentDirectory()}\backup\SomeRepo pull {https}://{username}:{token}@github.com/{organization}/SomeRepo.git";

                    break;

                case "clone":
                    expectedArguments = $@"clone {https}://{username}:{token}@github.com/{organization}/SomeRepo.git {Directory.GetCurrentDirectory()}\backup\SomeRepo";
                    break;

            }


            Assert.That(arg.Arguments, Is.EqualTo(expectedArguments), "Arguments");
            Assert.That(arg.WindowStyle, Is.EqualTo(ProcessWindowStyle.Hidden));
            Assert.That(arg.CreateNoWindow, Is.True);
            Assert.That(arg.RedirectStandardInput, Is.True);
            Assert.That(arg.RedirectStandardOutput, Is.True);
            Assert.That(arg.UseShellExecute, Is.False);

            var result = arg.FileName == programOptions.PathToGit
                                              && arg.Arguments == expectedArguments
                                              && arg.WindowStyle == ProcessWindowStyle.Hidden
                                              && arg.CreateNoWindow == true
                                              && arg.RedirectStandardInput == true
                                              && arg.RedirectStandardOutput == true
                                              && arg.UseShellExecute == false;

            return result;

        }

        [Test]
        public void BackupAllRepos_AllReposCallBackup()
        {
            // Arrange
            var clientInitializer = Substitute.For<GitClientFactory>();
            var logger = Substitute.For<ILog>();
            var processRunner = Substitute.For<ProcessRunner>(logger);

            
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = Builder<ProgramOptions>
                .CreateNew()
                .With(x => x.BackupLocation = backupLocation)
                .With(x => x.ProjectFilter = "")
                .Build();

            var gitApi = new GitApi(clientInitializer, processRunner, logger);
            gitApi.SetProgramOptions(programOptions);
            var allRepositories = Builder<Repository>
                .CreateListOfSize(3)
                .All().WithFactory(index => new Repository(index))
                .Build()
                .ToList();

            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer.CreateGitClient(programOptions.Username, programOptions.Token).Returns(repoClient);
            repoClient.GetAllForOrg(programOptions.Organization).Returns(task);


            // Act
            gitApi.BackupAllRepos();

            // Arrange
            foreach (var gitRepository in allRepositories)
            {
                processRunner.Received().Run(
                    Arg.Is<ProcessStartInfo>(s => s.Arguments.Contains(gitRepository.Name))
                    );
            }
        }
    }
}