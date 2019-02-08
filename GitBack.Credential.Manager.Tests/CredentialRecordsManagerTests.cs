using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NSubstitute;
using NUnit.Framework;

namespace GitBack.Credential.Manager.Tests
{
    public class CredentialRecordsManagerTests
    {
        private IMutex _mutex;
        private IEncryption _encryption;
        private IFileStreamer _fileStreamer;
        private IStreamFactory _streamFactory;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;
        
        private Stream _stream;

        [SetUp]
        public void BeforeEach()
        {
            _mutex = Substitute.For<IMutex>();
            _encryption = Substitute.For<IEncryption>();
            _fileStreamer = Substitute.For<IFileStreamer>();
            _logger = Substitute.For<ILogger>();
            _loggerFactory = Substitute.For<ILoggerFactory>();

            _loggerFactory.GetLogger(Arg.Any<Type>()).Returns(_logger);

            _streamFactory = Substitute.For<IStreamFactory>();
            _stream = new MemoryStream();
            _streamFactory.GetStream(Arg.Any<FileInfo>()).Returns(_stream);
        }

        [Test]
        public void ListRecords()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var emptyRecord = new CredentialRecord(_logger);
            var credentialRecordCollection = new List<ICredentialRecord>
            {
                new CredentialRecord(_logger) { Host = "Host", Protocol = "Https" },
                new CredentialRecord(_logger)
                {
                    Host = "Host", Protocol = "Https", Username = "user", Password = "password"
                },
            };

            var credentialRecordInfoCollection = credentialRecordCollection.Select(cr => cr.GetCredentialRecordInfo());

            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordInfoCollection);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            var result = credentialRecordsManager.ListRecords(emptyRecord);


            // assert
            Assert.That(result, Is.EquivalentTo(credentialRecordCollection.Select(cr => cr.ToString())));
        }

        [Test]
        public void EraseRecords()
        {
            // arrange
            _mutex.WaitOne().Returns(true);
            var userToErase = "userToErase";

            var recordsToErase = new CredentialRecord(_logger) { Username = userToErase };
            var credentialRecordCollection = new List<ICredentialRecordInfo>
            {
                new CredentialRecordInfo { Host = "Host", Protocol = "Https" },
                new CredentialRecordInfo { Host = "Host", Protocol = "Https", Username = "user", Password = "password" },
                new CredentialRecordInfo { Host = "Host", Protocol = "Https", Username = userToErase, Password = "password" },
            };
           
            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordCollection);
            var listOfRecords = new List<ICredentialRecordInfo>();
            _fileStreamer.StoreObjectToStream(Arg.Do<IEnumerable<ICredentialRecordInfo>>(lcr => ClearAndAdd(listOfRecords, lcr)), _stream);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            credentialRecordsManager.EraseRecords(recordsToErase);

            // assert
            Assert.That(listOfRecords, Is.Not.Empty);
            Assert.That(listOfRecords, Is.EquivalentTo(credentialRecordCollection.Where(r => !recordsToErase.GetCredentialRecordInfo().IsMatch(r))));
        }

        private static void ClearAndAdd(List<ICredentialRecordInfo> records, IEnumerable<ICredentialRecordInfo> recordsToAdd)
        {
            records.Clear();
            records.AddRange(recordsToAdd);
        }

        [Test]
        public void EraseRecords_EmptyMatchingRecord_Does_Nothing()
        {
            // arrange
            _mutex.WaitOne().Returns(true);
            var userToErase = "userToErase";

            var recordsToErase = new CredentialRecord(_logger);
            var credentialRecordCollection = new List<ICredentialRecordInfo>
            {
                new CredentialRecordInfo { Host = "Host", Protocol = "Https" },
                new CredentialRecordInfo { Host = "Host", Protocol = "Https", Username = "user", Password = "password" },
                new CredentialRecordInfo { Host = "Host", Protocol = "Https", Username = userToErase, Password = "password" },
            };

           
            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordCollection);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            credentialRecordsManager.EraseRecords(recordsToErase);

            // assert
            _fileStreamer.DidNotReceive().StoreObjectToStream(Arg.Any<List<ICredentialRecordInfo>>(), Arg.Any<Stream>());
        }

        [Test]
        public void StoreRecords_WhenYoungestIsStoredFirst_GetRecords_ReturnsYoungest()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var userToGet = "userToGet";
            var oldestRecordToStore = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "oldest-Path" };

            // force youngest Record to be noticeably younger.
            Thread.Sleep(100);
            var youngestRecordToStore = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "youngest-Path" };
            var recordToMatch = new CredentialRecord(_logger) { Username = userToGet };

            var credentialRecordCollection = new List<ICredentialRecordInfo>();

            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordCollection);
;
            _fileStreamer.StoreObjectToStream(Arg.Do<IEnumerable<ICredentialRecordInfo>>(l => ClearAndAdd(credentialRecordCollection, l)), _stream);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            credentialRecordsManager.StoreRecord(youngestRecordToStore);
            credentialRecordsManager.StoreRecord(oldestRecordToStore);
            var record = credentialRecordsManager.GetRecord(recordToMatch);

            // assert
            Assert.That(record, Is.EqualTo(youngestRecordToStore.GetOutputString()));
        }

        [Test]
        public void StoreRecords_WhenOldestIsStoredFirst_GetRecords_ReturnsYoungest()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var userToGet = "userToGet";
            var oldestRecordToStore = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "oldest-Path" };

            // force youngest Record to be noticeably younger.
            Thread.Sleep(100);
            var youngestRecordToStore = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "youngest-Path" };
            var recordToMatch = new CredentialRecord(_logger) { Username = userToGet };

            var credentialRecordCollection = new List<ICredentialRecordInfo>();
            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordCollection);

            _fileStreamer.StoreObjectToStream(Arg.Do<IEnumerable<ICredentialRecordInfo>>(l => ClearAndAdd(credentialRecordCollection, l)), _stream);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            credentialRecordsManager.StoreRecord(oldestRecordToStore);
            credentialRecordsManager.StoreRecord(youngestRecordToStore);
            var record = credentialRecordsManager.GetRecord(recordToMatch);

            // assert
            Assert.That(record, Is.EqualTo(youngestRecordToStore.GetOutputString()));
        }

        [Test]
        public void StoreRecords_Stores_All_Records()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var userToGet = "userToGet";
            var oldestRecordToStore = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "oldest-Path" };

            // force youngest Record to be noticeably younger.
            Thread.Sleep(100);
            var youngestRecordToStore = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "youngest-Path" };
            var recordToMatch = new CredentialRecord(_logger) { Username = userToGet };

            var credentialRecordCollection = new List<ICredentialRecordInfo>();
            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordCollection);

            _fileStreamer.StoreObjectToStream(Arg.Do<IEnumerable<ICredentialRecordInfo>>(l => ClearAndAdd(credentialRecordCollection, l)), _stream);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            credentialRecordsManager.StoreRecord(oldestRecordToStore);
            credentialRecordsManager.StoreRecord(youngestRecordToStore);
            var records = credentialRecordsManager.ListRecords(recordToMatch);

            // assert
            Assert.That(credentialRecordCollection, Is.Not.Empty);
            Assert.That(records, Is.EquivalentTo(credentialRecordCollection.Select(cri => new CredentialRecord(cri, _logger).ToString())));
        }

        [Test]
        public void StoreRecords_Stores_Encrypted_Passwords()
        {
            // arrange
            _mutex.WaitOne().Returns(true);
            var password = "password";
            var record = new CredentialRecord(_logger) { Host = "Host", Protocol = "Https", Username = "user", Password = password, Path = "oldest-Path" };

            var credentialRecordCollection = new List<ICredentialRecordInfo>();
            _fileStreamer.GetObjectFromStream(_stream).Returns(credentialRecordCollection);

            _fileStreamer.StoreObjectToStream(Arg.Do<IEnumerable<ICredentialRecordInfo>>(l => ClearAndAdd(credentialRecordCollection, l)), _stream);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            // act
            credentialRecordsManager.StoreRecord(record);

            // assert
            Assert.That(credentialRecordCollection, Is.Not.Empty);
            _encryption.Received().Encrypt(password);
        }

        [Test]
        public void GetCredentialRecordFromOptions_Empty()
        {
            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);

            var credentialHelperOptions = new CredentialHelperOptions();

            var record = credentialRecordsManager.GetCredentialRecordFromOptions(credentialHelperOptions);

            Assert.That(record.IsEmpty);
        }

        [Test]
        public void GetCredentialRecordFromOptions_Url_UpdatesOtherValues_IfNotProvided()
        {
            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);
            const string protocol = "http";
            const string username = "user";
            const string password = "pass";
            const string host = "example.com:82";
            const string path = "foo?bar#baz";

            var credentialHelperOptions = new CredentialHelperOptions { Url = $"{protocol}://{username}:{password}@{host}/{path}" };

            var record = credentialRecordsManager.GetCredentialRecordFromOptions(credentialHelperOptions);

            Assert.Multiple(() =>
            {
                Assert.That(record.Protocol, Is.EqualTo(protocol));
                Assert.That(record.Username, Is.EqualTo(username));
                Assert.That(record.Password, Is.EqualTo(password));
                Assert.That(record.Host, Is.EqualTo(host));
                Assert.That(record.Path, Is.EqualTo(path));
            });
        }

        [Test]
        public void GetCredentialRecordFromOptions_OtherValues_IfProvided()
        {
            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _streamFactory, _mutex, _encryption, _loggerFactory);
            const string urlProtocol = "http";
            const string expectedProtocol = "https";

            const string urlUsername = "user";
            const string expectedUsername = "user1";

            const string urlPassword = "pass";
            const string expectedPassword = "pass123";
            const string urlHost = "example.com:82";
            const string expectedHost = "example1.com";
            const string urlPath = "foo?bar#baz";
            const string expectedPath = "foo?bar#baz";

            var credentialHelperOptions = new CredentialHelperOptions
            {
                Protocol = expectedProtocol,
                Path = expectedPath,
                Url = $"{urlProtocol}://{urlUsername}:{urlPassword}@{urlHost}/{urlPath}",
                Username = expectedUsername,
                Password = expectedPassword,
                Host = expectedHost
            };

            var record = credentialRecordsManager.GetCredentialRecordFromOptions(credentialHelperOptions);

            Assert.Multiple(() =>
            {
                Assert.That(record.Protocol, Is.EqualTo(expectedProtocol));
                Assert.That(record.Username, Is.EqualTo(expectedUsername));
                Assert.That(record.Password, Is.EqualTo(expectedPassword));
                Assert.That(record.Host, Is.EqualTo(expectedHost));
                Assert.That(record.Path, Is.EqualTo(expectedPath));
            });
        }
    }
}
