using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace GitBack.Credential.Manager.Integration.Tests
{
    public class EndToEndTests
    {
        private static readonly Assembly CredentialManager = typeof(GitBack.Credential.Manager.Program).Assembly;
        private static readonly string CredentialManagerLocation = AssemblyHelper.GetAssemblyPath(CredentialManager);
        private static string _reportsDirectory;
        private string _testReportFile;

        private static readonly List<ReadonlyTestDataInput> TestDataInputs = new List<ReadonlyTestDataInput>
        {
            CreateTestDataInput(CredentialOptionUrl.FileUrl).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.RemoteFileUrl).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.HttpsUrl).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.FileUrl, CredentialOptions.Username | CredentialOptions.Password).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.RemoteFileUrl, CredentialOptions.Username | CredentialOptions.Password).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.HttpsUrl, CredentialOptions.Username | CredentialOptions.Password).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.HttpsUrl, CredentialOptions.Username | CredentialOptions.Password | CredentialOptions.Path).AsReadOnly(),
            CreateTestDataInput(CredentialOptionUrl.HttpsUrl, CredentialOptions.Username | CredentialOptions.UrlInvalidPassword).AsReadOnly(),
            CreateTestDataInput(CredentialOptions.Username | CredentialOptions.Password).AsReadOnly(),
            CreateTestDataInput(CredentialOptions.Username | CredentialOptions.Password | CredentialOptions.Host).AsReadOnly(),
            CreateTestDataInput(CredentialOptions.Username | CredentialOptions.Password | CredentialOptions.Host | CredentialOptions.Path).AsReadOnly(),
            CreateTestDataInput(CredentialOptions.Username | CredentialOptions.Password | CredentialOptions.Host | CredentialOptions.Path | CredentialOptions.Protocol).AsReadOnly(),
            CreateTestDataInput(CredentialOptions.Username | CredentialOptions.Password | CredentialOptions.Host | CredentialOptions.Path | CredentialOptions.Protocol, "file").AsReadOnly(),
        };

        private List<Process> _credentialManagerProcesses;

        [OneTimeSetUp]
        public static void BeforeAll()
        {
            //using (var mutex = new System.Threading.Mutex(false, $"{typeof(EndToEndTests).FullName}.{nameof(BeforeAll)}"))
            //{
            //    // make sure we get separate report directories when nchrunch parellezes.
            //    mutex.WaitOne();
            //    try
            //    {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    var context = TestContext.CurrentContext;
                    var workDir = context.WorkDirectory;
                    var date = DateTime.Now;
                    var hourSpan = TimeSpan.FromHours(date.TimeOfDay.Hours);
                    var seconds = (date.TimeOfDay - hourSpan).TotalSeconds;
                    _reportsDirectory = Path.Combine(workDir, $"{date.DayOfYear:000}", $"{hourSpan.Hours:00}.{seconds:0000}");
                    CleanUpReportsDirectory();
                //}
                //finally { mutex.ReleaseMutex(); }
            //}
        }

        [OneTimeTearDown]
        public static void AfterAll() => CleanUpReportsDirectory();

        private static void CleanUpReportsDirectory()
        {
            var reportsDirectory = new DirectoryInfo(_reportsDirectory);
            if (reportsDirectory.Exists) { TryToDeleteAll(reportsDirectory); }
        }

        [SetUp]
        public void BeforeEach()
        {
            var context = TestContext.CurrentContext;
            var testName = $"{context.Test.MethodName}.xml";

            _credentialManagerProcesses = new List<Process>();

             _testReportFile = Path.Combine(_reportsDirectory, testName);
             Console.Out.WriteLine($"_testReportFile: {_testReportFile}");
        }

        [TearDown]
        public void AfterEach() => CleanProcesses();

        [Test]
        public void CredentialManagerExe_Exit_Code_Not_0_With_No_Arguments()
        {
            // arrange
            var credentialManager = GetCredentialManagerProcess();

            // act
            credentialManager.Start();
            credentialManager.WaitForExit();

            var totalTime = credentialManager.TotalProcessorTime;
            Console.WriteLine($"Credential Manager Took {totalTime.TotalSeconds:f3} seconds to Exit");

            // assert
            Assert.That(credentialManager.HasExited, "Expected program to Exit");
            Assert.That(credentialManager.ExitCode, Is.Not.EqualTo(0));
        }

        [Test]
        public void CredentialManagerExe_Exit_Code_0_With_Help_Argument()
        {
            // arrange
            var credentialManager = GetCredentialManagerProcess("--help");

            // act
            credentialManager.Start();
            credentialManager.WaitForExit();

            var totalTime = credentialManager.TotalProcessorTime;
            Console.WriteLine($"Credential Manager Took {totalTime.TotalSeconds:f3} seconds to Exit");

            // assert
            Assert.That(credentialManager.HasExited, "Expected program to Exit");
            Assert.That(credentialManager.ExitCode, Is.EqualTo(0));
        }

        [Test]
        public void CredentialManagerExe_List()
        {
            // arrange
            StoreData(TestDataInputs);
            
            // act
            var credentialManager = GetCredentialManagerProcess("list");
            credentialManager.Start();

            // assert
            
            var credentials = credentialManager.StandardOutput.ReadToEnd();
            var errors = credentialManager.StandardError.ReadToEnd();
            Console.WriteLine($"credentialManager Output:\n{credentials}");
            Console.WriteLine($"credentialManager errors:\n{errors}");

            credentialManager.WaitForExit();

            var totalTime = credentialManager.TotalProcessorTime;
            Console.WriteLine($"Credential Manager Took {totalTime.TotalSeconds:f3} seconds to Exit");

            Assert.That(credentialManager.HasExited, "Expected program to Exit");

            Assert.Multiple(() =>
            {
                foreach (var expectedRecord in TestDataInputs.Select(t => t.ExpectedCredentialRecord))
                {
                    if (!string.IsNullOrEmpty(expectedRecord.Path))
                    {
                        var reverseSlashesPath = expectedRecord.Path.Replace(@"\", "/");
                        Assert.That(credentials, Contains.Substring(expectedRecord.Path).Or.ContainsSubstring(reverseSlashesPath));
                    }
                    if (!string.IsNullOrEmpty(expectedRecord.Host))
                    {
                        Assert.That(credentials, Contains.Substring(expectedRecord.Host));
                    }
                    if (!string.IsNullOrEmpty(expectedRecord.Password))
                    {
                        Assert.That(credentials, Contains.Substring(expectedRecord.Password));
                    }
                    if (!string.IsNullOrEmpty(expectedRecord.Protocol))
                    {
                        Assert.That(credentials, Contains.Substring(expectedRecord.Protocol));
                    }
                    if (!string.IsNullOrEmpty(expectedRecord.Username))
                    {
                        Assert.That(credentials, Contains.Substring(expectedRecord.Username));
                    }
                    if (!string.IsNullOrEmpty(expectedRecord.Url))
                    {
                        Assert.That(credentials, Contains.Substring(expectedRecord.Url));
                    }
                   
                }
            });
        }


        //private System.Threading.Mutex _mutex;

        [TestCaseSource(nameof(GetFilters))]
        public void CredentialManagerExe_Get(string filterBy, CredentialRecord expectedRecord)
        {
            //var fileLock = Path.Combine(_reportsDirectory, "file.lock");
            StoreData(TestDataInputs);
            //if (!File.Exists(_testReportFile))
            //{
            //    using (var mutex = new System.Threading.Mutex(false, _testReportFile.Replace(@"\", ":")))
            //    {
            //        mutex.WaitOne();
            //        try
            //        {
            //            if (!File.Exists(_testReportFile))
            //            {
            //                StoreData(TestDataInputs);
            //            }
            //        }
            //        finally
            //        {
            //            mutex.ReleaseMutex();
            //        }
            //    }
            //}

            var credentialManager = GetCredentialManagerProcess("get");
            credentialManager.Start();
            credentialManager.StandardInput.WriteLine(filterBy);
            EndInput(credentialManager);

            var credentials = credentialManager.StandardOutput.ReadToEnd();
            var errors = credentialManager.StandardError.ReadToEnd();
            Console.WriteLine($"credentialManager Output:\n{credentials}");
            Console.WriteLine($"credentialManager errors:\n{errors}");

            credentialManager.WaitForExit();

            var totalTime = credentialManager.TotalProcessorTime;
            Console.WriteLine($"Credential Manager Took {totalTime.TotalSeconds:f3} seconds to Exit");

            Assert.That(credentials.Trim(), Is.EqualTo(expectedRecord.GetOutputString().Trim()));

        }

        public static IEnumerable<TestCaseData> GetFilters()
        {
            var pathFilters = TestDataInputs.Where(t => !string.IsNullOrEmpty(t.ExpectedCredentialRecord.Path))
                                            .Select(t =>
                                             {
                                                 var filterBy = $"path={t.ExpectedCredentialRecord.Path}";
                                                 return new TestCaseData(filterBy, t.ExpectedCredentialRecord)
                                                 {
                                                     TestName = $"Get FilterBy: {filterBy}"
                                                 };
                                             });
            var hostFilters = TestDataInputs.Where(t => !string.IsNullOrEmpty(t.ExpectedCredentialRecord.Host))
                                            .Select(t =>
                                             {
                                                 var filterBy = $"host={t.ExpectedCredentialRecord.Host}";
                                                 return new TestCaseData(filterBy, t.ExpectedCredentialRecord)
                                                 {
                                                     TestName = $"Get FilterBy: {filterBy}"
                                                 };
                                             });
            var urlFilters = TestDataInputs.Where(t => !string.IsNullOrEmpty(t.ExpectedCredentialRecord.Url))
                                            .Select(t =>
                                             {
                                                 var filterBy = $"url={t.ExpectedCredentialRecord.Url}";
                                                 return new TestCaseData(filterBy, t.ExpectedCredentialRecord)
                                                 {
                                                     TestName = $"Get FilterBy: {filterBy}"
                                                 };
                                             });
            var userFilters = TestDataInputs.Where(t => !string.IsNullOrEmpty(t.ExpectedCredentialRecord.Username))
                                            .Select(t =>
                                             {
                                                 var filterBy = $"username={t.ExpectedCredentialRecord.Username}";
                                                 return new TestCaseData(filterBy, t.ExpectedCredentialRecord)
                                                 {
                                                     TestName = $"Get FilterBy: {filterBy}"
                                                 };
                                             });

            return pathFilters.Concat(hostFilters).Concat(urlFilters).Concat(userFilters);
        }

        private void StoreData(IEnumerable<ReadonlyTestDataInput> testDataInputs) => Parallel.ForEach(testDataInputs, StoreData);

        private static readonly ConcurrentDictionary<Tuple<string, ReadonlyTestDataInput>, int> storedData =
            new ConcurrentDictionary<Tuple<string, ReadonlyTestDataInput>, int>();
        private void StoreData(ReadonlyTestDataInput testDataInput)
        {
            var tuple = new Tuple<string, ReadonlyTestDataInput>(_testReportFile, testDataInput);
            if (!storedData.TryAdd(tuple, 1)) { return; }

            var storeByProcess = GetCredentialManagerProcess("store");
            storeByProcess.Start();
            Store(storeByProcess, testDataInput);
            EndInput(storeByProcess);

            var errors = storeByProcess.StandardError.ReadToEnd();
            Console.WriteLine($"credentialManager errors:\n{errors}");

            if (!storeByProcess.HasExited) { storeByProcess.WaitForExit(); }

            CleanProcess(storeByProcess);
            _credentialManagerProcesses.Remove(storeByProcess);
        }

        private static void Store(Process credentialManagerProcess, IEnumerable<StdKeyValueInput> stdKeyValueInputs)
        {
            var input = credentialManagerProcess.StandardInput;
            foreach (var stdKeyValueInput in stdKeyValueInputs)
            {
                input.WriteLine($"{stdKeyValueInput}");
            }
        }

        private static void EndInput(Process credentialManagerProcess)
        {
            var input = credentialManagerProcess.StandardInput;
            // The list of attributes is terminated by a blank line or end-of-file
            // see https://git-scm.com/docs/git-credential#IOFMT
            input.WriteLine();
            input.Flush();
        }

        private Process GetCredentialManagerProcess(string arguments = "", IDictionary<string, string> environmentVariables = null)
        {
            var workingDirectory = new FileInfo(_testReportFile).Directory;
            if (!workingDirectory.Exists)
            {
                workingDirectory.Create();
            }

            var startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = CredentialManagerLocation,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,

                WorkingDirectory = workingDirectory.FullName,
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
                environmentVariables.Add(locationVariableKey, _testReportFile);
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

        private static bool TryToDeleteAll(DirectoryInfo startingAt)
        {
            var returnValue = true;
            if (!startingAt.Exists) { return true; }

            var directoryContents = startingAt.GetFileSystemInfos();

            foreach (var item in directoryContents)
            {
                if (!item.Exists) { continue; }

                if (item is DirectoryInfo directory)
                {
                    if (!TryToDeleteAll(directory)) { returnValue = false; }
                }
                else
                {
                    try { item.Delete(); }
                    catch (IOException e)
                    {
                        WriteError($"Could Not Delete {item.FullName}: {e.Message}");
                        returnValue = false;
                    }
                }
            }

            try { startingAt.Delete(); }
            catch (IOException e)
            {
                WriteError($"Could Not Delete directory {startingAt.FullName}: {e.Message}");
                returnValue = false;
            }

            return returnValue;
        }

        private void CleanProcesses()
        {
            foreach (var process in _credentialManagerProcesses) { CleanProcess(process); }

            _credentialManagerProcesses.Clear();
        }

        private static void CleanProcess(Process process)
        {
            using (process)
            {
                if (process == null) { return; }

                // we don't want the AfterEach to fail and hide errors
                // closing the process does not close its streams in case they are needed later
                try { process.StandardError?.Close(); } catch(Exception e) { WriteError($"Could Not Close StandardError, did you start the process?\n  Error: {e.Message}");}

                try { process.StandardInput?.Close(); } catch(Exception e) { WriteError($"Could Not Close StandardInput, did you start the process?\n  Error:  {e.Message}");}

                try { process.StandardOutput?.Close(); } catch(Exception e) { WriteError($"Could Not Close StandardOutput, did you start the process?\n  Error:  {e.Message}");}

                try
                {
                    process.WaitForExit(10000);
                    if (!process.HasExited) { process.Kill(); }

                    process.WaitForExit();
                }
                catch (Exception e)
                {
                    WriteError($"Could Not kill process, did you start the process?\n  Error:  {e.Message}");
                }
            }
        }

        private static void WriteError(string message, Exception exception = null)
        {
            if (!string.IsNullOrEmpty(message) && exception != null)
            {
                Console.Error.WriteLine($"{message}\n{exception}");
            }
            else if (!string.IsNullOrEmpty(message))
            {
                Console.Error.WriteLine($"{message}");
            }
            else if (exception != null)
            {
                Console.Error.WriteLine($"{exception}");
            }
        }

        private static int _count = 0;

        [Flags]
        public enum CredentialOptions
        {
            None = 0,
            Username = 1 << 1,
            Password = 1 << 2,
            UrlInvalidPassword = 1 << 3,
            Host = 1 << 4,
            Path = 1 << 5,
            Protocol = 1 << 6
        }

        public enum CredentialOptionUrl
        {
            None,
            HttpsUrl,                         // includes protocol and host
            FileUrl,                          // includes protocol and path
            RemoteFileUrl,                    // includes protocol, host and path
        }

        private static TestDataInput CreateTestDataInput(CredentialOptions options, string protocol = "https") => CreateTestDataInput(CredentialOptionUrl.None, options, protocol);

        private static TestDataInput CreateTestDataInput(CredentialOptionUrl urlOption = CredentialOptionUrl.None, CredentialOptions options = CredentialOptions.None, string protocol = "https")
        {
            Interlocked.Increment(ref _count);
            var username = $"user{_count}";
            var password = $"pass{_count}";
            var host = $"host{_count}.example.com";
            var path = $"path\\to\\repo{_count}";
            var dataInput = new TestDataInput();

            switch (urlOption)
            {
                case CredentialOptionUrl.HttpsUrl: dataInput.Add("url", $"https://{host}"); break;
                case CredentialOptionUrl.FileUrl: dataInput.Add("url", $@"C:\\{path}"); break;
                case CredentialOptionUrl.RemoteFileUrl: dataInput.Add("url", $@"\\{host}\{path}"); break;
                default: break;
            }

            if (options.HasFlag(CredentialOptions.Host))
            {
                dataInput.Add("host", host);
            }

            if (options.HasFlag(CredentialOptions.Path))
            {
                dataInput.Add("path", path);
            }

            if (options.HasFlag(CredentialOptions.Password))
            {
                dataInput.Add("password", password);
            }

            if (options.HasFlag(CredentialOptions.UrlInvalidPassword))
            {
                dataInput.Add("password", $"p@ss{_count}");
            }

            if (options.HasFlag(CredentialOptions.Username))
            {
                dataInput.Add("username", username);
            }

            if (options.HasFlag(CredentialOptions.Protocol))
            {
                dataInput.Add("protocol", protocol);
            }

            return dataInput;
        }

        public class TestDataInput : ICollection<StdKeyValueInput>
        {
            private static readonly ILogger Logger = Substitute.For<ILogger>();

            private readonly IList<StdKeyValueInput> _stdKeyValueInputs = new List<StdKeyValueInput>();

            public CredentialRecord ExpectedCredentialRecord { get; private set; } = new CredentialRecord(Logger);
            public IEnumerator<StdKeyValueInput> GetEnumerator() => _stdKeyValueInputs.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(string key, string value) => Add(new StdKeyValueInput(key, value));

            public void Add(StdKeyValueInput item)
            {
                _stdKeyValueInputs.Add(item);
               AddToRecord(item);
            }

            private void AddToRecord(StdKeyValueInput item) => ExpectedCredentialRecord.AddOrUpdatePropertyValue(item.Key, item.Value);

            public void Clear()
            {
                _stdKeyValueInputs.Clear();
                ClearRecord();
            }

            private void ClearRecord() => ExpectedCredentialRecord = new CredentialRecord(Logger);

            public bool Contains(StdKeyValueInput item) => _stdKeyValueInputs.Contains(item);

            public void CopyTo(StdKeyValueInput[] array, int arrayIndex) => _stdKeyValueInputs.CopyTo(array, arrayIndex);

            public bool Remove(StdKeyValueInput item)
            {
                if (!_stdKeyValueInputs.Remove(item)) { return false; }

                ClearRecord();
                foreach (var stdKeyValueInput in _stdKeyValueInputs)
                {
                    AddToRecord(stdKeyValueInput);
                }

                return true;
            }

            public int Count => _stdKeyValueInputs.Count;
            public bool IsReadOnly => _stdKeyValueInputs.IsReadOnly;

            public ReadonlyTestDataInput AsReadOnly() => new ReadonlyTestDataInput(this);
        }

        public class ReadonlyTestDataInput : IReadOnlyCollection<StdKeyValueInput>, IEquatable<ReadonlyTestDataInput>
        {
            public CredentialRecord ExpectedCredentialRecord { get; }
            private readonly IReadOnlyList<StdKeyValueInput> _stdKeyValueInputs;

            public ReadonlyTestDataInput(TestDataInput testDataInput) : this(testDataInput, testDataInput?.ExpectedCredentialRecord) { }

            public ReadonlyTestDataInput(IEnumerable<StdKeyValueInput> stdKeyValueInputs, CredentialRecord expectedCredentialRecord)
            {
                ExpectedCredentialRecord = expectedCredentialRecord;
                _stdKeyValueInputs = (stdKeyValueInputs ?? Enumerable.Empty<StdKeyValueInput>()).ToList();
            }

            public IEnumerator<StdKeyValueInput> GetEnumerator() => _stdKeyValueInputs.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _stdKeyValueInputs.Count;

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 17 + ExpectedCredentialRecord?.ToString().GetHashCode() ?? 0;
                    hashCode += Count * 23;
                    foreach (var stdKeyValueHashCode in _stdKeyValueInputs.Where(s => s != null).Select(s => s.ToString().GetHashCode()))
                    {
                        hashCode += 23 * stdKeyValueHashCode;
                    }
                    return hashCode;
                }
            }

            public bool Equals(ReadonlyTestDataInput other)
            {
                if (other == null) { return false; }
                if (ReferenceEquals(this, other)) { return true; }

                if (Count != other.Count || ExpectedCredentialRecord?.ToString() != other.ExpectedCredentialRecord.ToString())
                {
                    return false;
                }

                foreach (var zippedPair in this.Zip(other, (mine, theirs) => new { Mine = mine, Theirs = theirs }))
                {
                    var mine = zippedPair.Mine;
                    var theirs = zippedPair.Theirs;
                    if (ReferenceEquals(mine, theirs)) { continue; }

                    if (mine?.ToString() != theirs?.ToString()) { return false; }
                }

                return true;
            }

            public override bool Equals(object obj) => Equals(obj as ReadonlyTestDataInput);
        }

        public class StdKeyValueInput
        {
            public string Key { get; }
            public string Value { get; }

            public StdKeyValueInput(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString() => $"{Key}={Value}";
        }
    }
}
