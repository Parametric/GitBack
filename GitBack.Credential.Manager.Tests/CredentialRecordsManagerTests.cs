using System.Collections.Generic;
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
        private IFileStreamer<List<ICredentialRecord>> _fileStreamer;
        private string _fileLocation;

        [SetUp]
        public void BeforeEach()
        {
            _mutex = Substitute.For<IMutex>();
            _encryption = Substitute.For<Manager.IEncryption>();
            _fileStreamer = Substitute.For<IFileStreamer<List<ICredentialRecord>>>();
            _fileLocation = "fileLocation";
        }

        [Test]
        public void ListRecords()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var emptyRecord = new CredentialRecord();
            var credentialRecordCollection = new List<ICredentialRecord>
            {
                new CredentialRecord { Host = "Host", Protocol = "Https" },
                new CredentialRecord
                {
                    Host = "Host", Protocol = "Https", Username = "user", Password = "password"
                },
            };

            _fileStreamer.GetObjectOfType(_fileLocation)
                        .Returns(credentialRecordCollection);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

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

            var recordsToErase = new CredentialRecord { Username = userToErase };
            var CredentialRecordCollection = new List<ICredentialRecord>
            {
                new CredentialRecord { Host = "Host", Protocol = "Https" },
                new CredentialRecord { Host = "Host", Protocol = "Https", Username = "user", Password = "password" },
                new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToErase, Password = "password" },
            };

           
            _fileStreamer.GetObjectOfType(_fileLocation).Returns(CredentialRecordCollection);
            var listOfRecords = new List<ICredentialRecord>();
            _fileStreamer.StoreObjectOfType(Arg.Do<List<ICredentialRecord>>( lcr => { listOfRecords.AddRange(lcr); }), _fileLocation);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

            // act
            credentialRecordsManager.EraseRecords(recordsToErase);

            // assert
            Assert.That(listOfRecords, Is.Not.Empty);
            Assert.That(listOfRecords, Is.EquivalentTo(CredentialRecordCollection.Where(r => !recordsToErase.IsMatch(r))));
        }

        [Test]
        public void EraseRecords_EmptyMatchingRecord_Does_Nothing()
        {
            // arrange
            _mutex.WaitOne().Returns(true);
            var userToErase = "userToErase";

            var recordsToErase = new CredentialRecord { };
            var CredentialRecordCollection = new List<ICredentialRecord>
            {
                new CredentialRecord { Host = "Host", Protocol = "Https" },
                new CredentialRecord { Host = "Host", Protocol = "Https", Username = "user", Password = "password" },
                new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToErase, Password = "password" },
            };

           
            _fileStreamer.GetObjectOfType(_fileLocation).Returns(CredentialRecordCollection);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

            // act
            credentialRecordsManager.EraseRecords(recordsToErase);

            // assert
            _fileStreamer.DidNotReceive().StoreObjectOfType(Arg.Any<List<ICredentialRecord>>(), Arg.Any<string>());
        }

        [Test]
        public void StoreRecords_WhenYoungestIsStoredFirst_GetRecords_ReturnsYoungest()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var userToGet = "userToGet";
            var oldestRecordToStore = new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "oldest-Path" };

            // force youngest Record to be noticeably younger.
            Thread.Sleep(100);
            var youngestRecordToStore = new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "youngest-Path" };
            var recordToMatch = new CredentialRecord { Username = userToGet };

            var CredentialRecordCollection = new List<ICredentialRecord>();
            _fileStreamer.GetObjectOfType(_fileLocation)
                        .Returns(CredentialRecordCollection);

            _fileStreamer.StoreObjectOfType(Arg.Do<List<ICredentialRecord>>(l =>
            {
                CredentialRecordCollection.Clear();
                CredentialRecordCollection.AddRange(l);
            }), _fileLocation);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

            // act
            credentialRecordsManager.StoreRecord(youngestRecordToStore);
            credentialRecordsManager.StoreRecord(oldestRecordToStore);
            var record = credentialRecordsManager.GetRecord(recordToMatch);

            // assert
            Assert.That(record, Is.EqualTo(youngestRecordToStore.ToString()));
        }

        [Test]
        public void StoreRecords_WhenOldestIsStoredFirst_GetRecords_ReturnsYoungest()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var userToGet = "userToGet";
            var oldestRecordToStore = new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "oldest-Path" };

            // force youngest Record to be noticeably younger.
            Thread.Sleep(100);
            var youngestRecordToStore = new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "youngest-Path" };
            var recordToMatch = new CredentialRecord { Username = userToGet };

            var CredentialRecordCollection = new List<ICredentialRecord>();
            _fileStreamer.GetObjectOfType(_fileLocation)
                        .Returns(CredentialRecordCollection);

            _fileStreamer.StoreObjectOfType(Arg.Do<List<ICredentialRecord>>(l =>
            {
                CredentialRecordCollection.Clear();
                CredentialRecordCollection.AddRange(l);
            }), _fileLocation);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

            // act
            credentialRecordsManager.StoreRecord(oldestRecordToStore);
            credentialRecordsManager.StoreRecord(youngestRecordToStore);
            var record = credentialRecordsManager.GetRecord(recordToMatch);

            // assert
            Assert.That(record, Is.EqualTo(youngestRecordToStore.ToString()));
        }

        [Test]
        public void StoreRecords_Stores_All_Records()
        {
            // arrange
            _mutex.WaitOne().Returns(true);

            var userToGet = "userToGet";
            var oldestRecordToStore = new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "oldest-Path" };

            // force youngest Record to be noticeably younger.
            Thread.Sleep(100);
            var youngestRecordToStore = new CredentialRecord { Host = "Host", Protocol = "Https", Username = userToGet, Password = "password", Path = "youngest-Path" };
            var recordToMatch = new CredentialRecord { Username = userToGet };

            var CredentialRecordCollection = new List<ICredentialRecord>();
            _fileStreamer.GetObjectOfType(_fileLocation)
                        .Returns(CredentialRecordCollection);

            _fileStreamer.StoreObjectOfType(Arg.Do<List<ICredentialRecord>>(l =>
            {
                CredentialRecordCollection.Clear();
                CredentialRecordCollection.AddRange(l);
            }), _fileLocation);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

            // act
            credentialRecordsManager.StoreRecord(oldestRecordToStore);
            credentialRecordsManager.StoreRecord(youngestRecordToStore);
            var records = credentialRecordsManager.ListRecords(recordToMatch).ToList();

            // assert
            Assert.That(CredentialRecordCollection, Is.Not.Empty);
            Assert.That(records, Is.EquivalentTo(CredentialRecordCollection.Select(cr => cr.ToString())));
        }

        [Test]
        public void StoreRecords_Stores_Encrypted_Passwords()
        {
            // arrange
            _mutex.WaitOne().Returns(true);
            var password = "password";
            var record = new CredentialRecord { Host = "Host", Protocol = "Https", Username = "user", Password = password, Path = "oldest-Path" };

            var CredentialRecordCollection = new List<ICredentialRecord>();
            _fileStreamer.GetObjectOfType(_fileLocation)
                         .Returns(CredentialRecordCollection);

            _fileStreamer.StoreObjectOfType(Arg.Do<List<ICredentialRecord>>(l =>
            {
                CredentialRecordCollection.Clear();
                CredentialRecordCollection.AddRange(l);
            }), _fileLocation);

            var credentialRecordsManager = new CredentialRecordsManager(_fileStreamer, _fileLocation, _mutex, _encryption);

            // act
            credentialRecordsManager.StoreRecord(record);

            // assert
            Assert.That(CredentialRecordCollection, Is.Not.Empty);
            _encryption.Received().Encrypt(password);
        }
    }
}
