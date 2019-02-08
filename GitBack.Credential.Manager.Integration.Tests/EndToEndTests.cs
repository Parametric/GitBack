using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace GitBack.Credential.Manager.Integration.Tests
{
    public class EndToEndTests
    {

        private static readonly Assembly CredentialManager = typeof(GitBack.Credential.Manager.Program).Assembly;
        private static readonly string CredentialManagerLocation = AssemblyHelper.GetAssemblyPath(CredentialManager);
        private static readonly string IntegrationTestsDirectory = Path.Combine(Path.GetDirectoryName(CredentialManagerLocation), "IntegrationTests");


        private List<Process> _credentialManagerProcesses;
        private string _testFolderName;


        [OneTimeSetUp]
        public static void BeforeAll() => CleanIntegrationTestsDirectory();

        [SetUp]
        public void BeforeEach()
        {
            _credentialManagerProcesses = new List<Process>();
            _testFolderName = null;
        }

        [TearDown]
        public void AfterEach()
        {
            CleanProcesses();
        }

        [Test]
        public void CredentialManagerExe_Exit_Code_Not_0_With_No_Arguments()
        {
            // arrange
            _testFolderName = nameof(CredentialManagerExe_Exit_Code_Not_0_With_No_Arguments);
            var credentialManager = GetCredentialManagerProcess();

            // act
            credentialManager.Start();

            // assert
            Assert.That(credentialManager.WaitForExit(5000), "Expected program to Exit");
            Assert.That(credentialManager.ExitCode, Is.Not.EqualTo(0));
        }

        [Test]
        public void CredentialManagerExe_Exit_Code_0_With_Help_Argument()
        {
            // arrange
            _testFolderName = nameof(CredentialManagerExe_Exit_Code_0_With_Help_Argument);
            var credentialManager = GetCredentialManagerProcess("--help");

            // act
            credentialManager.Start();

            // assert
            Assert.That(credentialManager.WaitForExit(5000), "Expected program to Exit");
            Assert.That(credentialManager.ExitCode, Is.EqualTo(0));
        }

        [Test]
        public void CredentialManagerExe_Store()
        {
            // arrange
            // using list instead of keyValuePairs as order matters.
            _testFolderName = nameof(CredentialManagerExe_Store);
            var user1 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", "user1"),
                new KeyValuePair<string, string>("password", "p@ssword"),
                new KeyValuePair<string, string>("host", "host.example.com"),
                new KeyValuePair<string, string>("protocol", "https"),
            };
            var user2 = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", "user2"),
                new KeyValuePair<string, string>("url", "https://url.example.com"),
                new KeyValuePair<string, string>("password", "p1ssword"),
            };

            var storeByUrl = GetCredentialManagerProcess("store");
            var storeByHost = GetCredentialManagerProcess("store");
            var list = GetCredentialManagerProcess("list --Listen No");
            storeByUrl.Start();
            storeByHost.Start();
            
            // act
            Store(storeByHost, user1);
            Store(storeByUrl, user2);

            EndInput(storeByUrl);
            EndInput(storeByHost);

            // assert
            list.Start();
            var credentials = list.StandardOutput.ReadToEnd();
            Console.Out.WriteLine(credentials);

            Assert.Multiple(() =>
            {
                Assert.That(storeByUrl.HasExited, "Expected program to Exit");
                Assert.That(storeByHost.HasExited, "Expected program to Exit");
                Assert.That(list.HasExited, "Expected program to Exit");
            });

            Assert.Multiple(() =>
            {
                foreach (var user2Value in user2.Select(v => v.Value))
                {
                    if (Uri.TryCreate(user2Value, UriKind.Absolute, out var uri))
                    {
                        var host = uri.Host;
                        var protocol = uri.Scheme;
                        Assert.That(credentials, Contains.Substring(host));
                        Assert.That(credentials, Contains.Substring(protocol));
                    }
                    else
                    {
                        Assert.That(credentials, Contains.Substring(user2Value));
                    }
                }

                foreach (var user1Value in user1.Select(v => v.Value))
                {
                    Assert.That(credentials, Contains.Substring(user1Value));
                }
            });
        }

        private static void Store(Process credentialManagerProcess, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            var input = credentialManagerProcess.StandardInput;
            Console.WriteLine("-----");
            foreach (var keyValue in keyValuePairs)
            {
                Console.WriteLine($"Writing: {keyValue.Key}={keyValue.Value}");
                input.WriteLine($"{keyValue.Key}={keyValue.Value}");
            }
        }

        private static bool EndInput(Process credentialManagerProcess)
        {
            var input = credentialManagerProcess.StandardInput;
            // The list of attributes is terminated by a blank line or end-of-file
            // see https://git-scm.com/docs/git-credential#IOFMT
            input.WriteLine();
            input.Flush();
            return credentialManagerProcess.WaitForExit(5000);
        }

        private Process GetCredentialManagerProcess(string arguments = null, IDictionary<string, string> environmentVariables = null)
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = CredentialManagerLocation,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,

                WorkingDirectory = IntegrationTestsDirectory,
                CreateNoWindow = true,
                ErrorDialog = false,
            };

            if (environmentVariables == null)
            {
                environmentVariables = new Dictionary<string, string>();
            }

            const string locationVariableKey = CredentialRecordsManager.RecordEnvironmentVariable;
            if (!environmentVariables.ContainsKey(locationVariableKey))
            {
                Assume.That(_testFolderName != null, "Each test should have a unique folder name, so tests running in parallel don't step on each other.");
                var testPath = Path.Combine(IntegrationTestsDirectory, _testFolderName);
                environmentVariables.Add(locationVariableKey, testPath);
            }

            var startingVariables = startInfo.EnvironmentVariables;
            foreach (var environmentVariable in environmentVariables)
            {
                if (startingVariables.ContainsKey(environmentVariable.Key))
                {
                    startingVariables[environmentVariable.Key] = environmentVariable.Value;
                }
                else
                {
                    startingVariables.Add(environmentVariable.Key, environmentVariable.Value);
                }
            }

            var process = new Process
            {
                StartInfo = startInfo
            };

            _credentialManagerProcesses.Add(process);
            return process;
        }

        private static void CleanIntegrationTestsDirectory()
        {
            var testDirectory = new DirectoryInfo(IntegrationTestsDirectory);

            if (!testDirectory.Exists)
            {
                testDirectory.Create();
            }

            foreach (var file in testDirectory.EnumerateFiles()) { file.Delete(); }
            foreach (var dir in testDirectory.EnumerateDirectories()) { dir.Delete(true); }
        }

        private void CleanProcesses()
        {
            foreach (var process in _credentialManagerProcesses)
            {
                using (process)
                {
                    if (process == null) { continue; }

                    // we don't want the AfterEach to fail and hide errors
                    // closing the process does not close its streams in case they are needed later
                    try { process.StandardError?.Close(); } catch(Exception e) { WriteError($"Could Not Close StandardError, did you start the process?\n  Error: {e.Message}");}
                    try { process.StandardInput?.Close(); } catch(Exception e) { WriteError($"Could Not Close StandardInput, did you start the process?\n  Error:  {e.Message}");}
                    try { process.StandardOutput?.Close(); } catch(Exception e) { WriteError($"Could Not Close StandardOutput, did you start the process?\n  Error:  {e.Message}");}

                    try
                    {
                        if (!process.HasExited) { process.Kill(); }
                        process.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        WriteError($"Could Not kill process, did you start the process?\n  Error:  {e.Message}");
                    }

                    
                }
            }
            _credentialManagerProcesses.Clear();
        }

        private static void WriteError(string message) => Console.Error.WriteLine(message);
    }
}
