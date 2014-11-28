﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NSubstitute;
using NUnit.Framework;
using Octokit;
using PPA.Logging.Contract;

namespace PPA.GitBack.Tests
{
    [TestFixture]
    class GitApiTests
    {
        [Test]
        public void GetUsername_ReturnsCorrectUsername()
        {
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = "password"
            };

            var gitApi = new GitApi(programOptions, null, null, null); 

            // Act
            var username = gitApi.GetUsername();
            var result = username.Equals("username"); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetOrganization_ReturnsCorrectOrganization()
        {
            // Arrange
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = "password"
            };

            var gitApi = new GitApi(programOptions, null, null, null);

            // Act
            var organization = gitApi.GetOrganization();
            var result = organization.Equals("organization");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetBackupLocation_ReturnsCorrectBackupLocation()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup"); 

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = backupLocation,
                Password = "password"
            };

           
            var gitApi = new GitApi(programOptions, null, null, null);

            // Act
            var backup = gitApi.GetBackupLocation();
            var result = backup.Equals(backupLocation); 

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetPassword_ReturnsCorrectPassword()
        {
            // Arrange
            const string password = "password";

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                Password = password
            };


            var gitApi = new GitApi(programOptions, null, null, null);

            // Act
            var backup = gitApi.GetPassword();
            var result = backup.Equals(password);

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
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = organization,
                BackupLocation = backupLocation,
            };

            var repoClient = Substitute.For<IRepositoriesClient>();
            var clientInitializer = Substitute.For<GitClientFactory>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var gitApi = new GitApi(programOptions, clientInitializer, null, null);

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForCurrent();
        }

        [Test]
        public void GetRepositories_FromOrganization_WhenOrganizationIsSpecified()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = "organization",
                BackupLocation = backupLocation,
            };


            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var gitApi = new GitApi(programOptions, clientInitializer, null, null);

            // Act
            gitApi.GetRepositories();

            // Assert
            repoClient.Received().GetAllForOrg(programOptions.Organization);
        }

        [Test]
        public void GetRepositories_MapsResultsToGitRepositoryObjects()
        {
            // Arrange
            var backupLocation = new DirectoryInfo("backup");
            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = null,
                BackupLocation = backupLocation,
            }; 
            
            var clientInitializer = Substitute.For<GitClientFactory>();
            var repoClient = Substitute.For<IRepositoriesClient>();
            clientInitializer
                .CreateGitClient(programOptions.Username, programOptions.Password)
                .Returns(repoClient)
                ;

            var allRepositories = Builder<Repository>.CreateListOfSize(2).Build().ToList();
            var task = new Task<IReadOnlyList<Repository>>(allRepositories.AsReadOnly);
            task.RunSynchronously();

            repoClient.GetAllForCurrent().Returns(task);

            var gitApi = new GitApi(programOptions, clientInitializer, null, null);

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
        public void Pull()
        {
            // Arrange
            var clientInitializer = Substitute.For<GitClientFactory>();
            var processRunner = Substitute.For<ProcessRunner>();
            var logger = Substitute.For<ILogger>();

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                PathToGit = "//some/path/to/git.exe"
            };

            var gitApi = new GitApi(programOptions, clientInitializer, processRunner, logger);

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
            var processRunner = Substitute.For<ProcessRunner>();
            var logger = Substitute.For<ILogger>();

            var programOptions = new ProgramOptions()
            {
                Username = "username",
                Password = "password",
                Organization = "organization",
                BackupLocation = new DirectoryInfo("backup"),
                PathToGit = "//some/path/to/git.exe"
            };

            var gitApi = new GitApi(programOptions, clientInitializer, processRunner, logger);

            // Act
            gitApi.Clone("SomeRepo");

            // Assert
            processRunner.Received().Run(Arg.Is<ProcessStartInfo>(arg => IsMatchingProcessStartInfo(arg, programOptions, "clone")));
        }

        private static bool IsMatchingProcessStartInfo(ProcessStartInfo arg, ProgramOptions programOptions, string gitCommand)
        {
            var expectedArguments = gitCommand +
                                    @" https://username:password@github.com/organization/SomeRepo.git backup\SomeRepo";

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
    }
}