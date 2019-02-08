using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Ninject.Infrastructure.Language;

namespace GitBack.Credential.Manager {
    public class CredentialRecordsManager : ICredentialRecordsManager
    {
        public const string RecordEnvironmentVariable = "GitBackRecordsLocation";
        public const string DefaultRecordDirectoryName = ".GitBack";
        public const string DefaultRecordFileName = "gitback.credentials.xml";

        private readonly IFileStreamer _fileStreamer;
        private readonly IMutex _recordsLock;
        private readonly IEncryption _encryption;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IStreamFactory _streamFactory;

        public CredentialRecordsManager(IFileStreamer fileStreamer, IStreamFactory streamFactory, IMutex recordsLock, IEncryption encryption, ILoggerFactory loggerFactory)
        {
            _fileStreamer = fileStreamer;
            _streamFactory = streamFactory;
            _recordsLock = recordsLock;
            _encryption = encryption;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.GetLogger(typeof(CredentialRecordsManager));
        }

        private FileInfo GetDefaultRecordsLocation()
        {
            var environmentValue = Environment.GetEnvironmentVariable(RecordEnvironmentVariable);
            _logger.Info($"Looking for {RecordEnvironmentVariable} Environment Variable for the GitBack credential records location.");

            var location = environmentValue != null
                ? Environment.ExpandEnvironmentVariables(environmentValue)
                : DefaultRecordDirectoryName;

            return GetRecordsLocation(location);
        }

        private FileInfo GetRecordsLocation(string location)
        {
            if (!Path.IsPathRooted(location))
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var newPath = Path.Combine(userProfile, location);
                _logger.Info($"{location} not rooted, adjusting to {newPath}");
                location = newPath;
            }

            if (File.Exists(location))
            {
                _logger.Info($"Found File at {location}, using it as the GitBack credential records location.");
                return new FileInfo(location);
            }

            var looksLikeADirectory = string.IsNullOrEmpty(Path.GetExtension(location));
            var looksLikeAHiddenDirectory = string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(location));

            if (Directory.Exists(location) || looksLikeADirectory || looksLikeAHiddenDirectory)
            {
                var filePath = Path.Combine(location, DefaultRecordFileName);
                _logger.Info($"{location} is not file, but is (or could be) a directory. Using {filePath} as the GitBack credential records location.");
                return new FileInfo(filePath);
            }

            _logger.Info($"Using {location} as the GitBack credential records location. (No file currently exists there, yet)");
            return new FileInfo(location);
        }

        private FileInfo GetRecordsLocation(FileSystemInfo location) => GetRecordsLocation(location.FullName);

        private FileInfo _recordsLocation;

        public FileInfo RecordsLocation
        {
            get => _recordsLocation ?? (_recordsLocation = GetDefaultRecordsLocation());

            set => _recordsLocation = GetRecordsLocation(value);
        }

        public IEnumerable<string> ListRecords(ICredentialRecord match)
        {
            var result = LockAndDo(GetStoredCredentials);
            return result.Where(match.IsMatch).Select(DecryptPassword).Select(r => r.ToString());
        }

        private ICredentialRecord DecryptPassword(ICredentialRecord credentialRecord)
        {
            var recordInfo = credentialRecord.GetCredentialRecordInfo();
            if (recordInfo.IsPasswordEncrypted)
            {
                recordInfo.Password = _encryption.Decrypt(credentialRecord.Password);
                recordInfo.IsPasswordEncrypted = false;
            }

            return credentialRecord;
        }

        public void EraseRecords(ICredentialRecord match)
        {
            // Don't delete all records in the faces of any empty matching record.
            if (match.IsEmpty()) { return; }
            LockAndDo(() => InternalEraseRecords(match));
        }

        private IEnumerable<string> InternalEraseRecords(ICredentialRecord record)
        {
            var records = GetStoredCredentials();
            var recordsToKeeps = records.Where(r => !record.IsMatch(r)).ToList();
            SaveRecords(recordsToKeeps);

            return Enumerable.Empty<string>();
        }

        public string GetRecord(ICredentialRecord match)
        {
            var records = GetRecords(match);
            var record = records.FirstOrDefault()?.GetOutputString() ?? string.Empty;
            return record;
        }

        private IEnumerable<ICredentialRecord> GetRecords(ICredentialRecord match)
        {
            var result = LockAndDo(GetStoredCredentials);
            return result.Where(match.IsMatch).Select(DecryptPassword);
        }
        
        public void StoreRecord(ICredentialRecord record)
        {
            if (record.IsEmpty()) { return; }

            var recordInfo = record.GetCredentialRecordInfo();
            if (!recordInfo.IsPasswordEncrypted)
            {
                recordInfo.Password = _encryption.Encrypt(recordInfo.Password);
                recordInfo.IsPasswordEncrypted = true;
            }
            LockAndDo(() => InternalStoreRecord(record));
        }

        public ICredentialRecord GetCredentialRecordFromOptions(CredentialHelperOptions credentialHelperOptions)
        {
            var record = new CredentialRecord(_loggerFactory.GetLogger(typeof(CredentialRecord)))
            {
                Url = credentialHelperOptions.Url,
            };

            if (!string.IsNullOrEmpty(credentialHelperOptions.Host)) { record.Host = credentialHelperOptions.Host; }
            if (!string.IsNullOrEmpty(credentialHelperOptions.Username)) { record.Username = credentialHelperOptions.Username; }
            if (!string.IsNullOrEmpty(credentialHelperOptions.Password)) { record.Password = credentialHelperOptions.Password; }
            if (!string.IsNullOrEmpty(credentialHelperOptions.Path)) { record.Path = credentialHelperOptions.Path; }
            if (!string.IsNullOrEmpty(credentialHelperOptions.Protocol)) { record.Protocol = credentialHelperOptions.Protocol; }

            return record;
        }

        private IEnumerable<string> InternalStoreRecord(ICredentialRecord record)
        {
            var records = GetStoredCredentials().ToList();

            records.Insert(0, record);
            SaveRecords(records);

            return Enumerable.Empty<string>();
        }

        private void SaveRecords(List<ICredentialRecord> recordsToSave)
        {
            recordsToSave.Sort(CompareRecordsByLastUpdate);

            var recordInfosToSave = recordsToSave.Select(r => r.GetCredentialRecordInfo());
            using (var stream = _streamFactory.GetStream(RecordsLocation))
            {
                 _fileStreamer.StoreObjectToStream(recordInfosToSave, stream);
            }
        }

        private static int CompareRecordsByLastUpdate(ICredentialRecord x, ICredentialRecord y)
        {
            if (ReferenceEquals(x, y)) { return 0; }

            if (x is null) { return -1; }

            if (y is null) { return 1; }

            var xLastUpdated = x.LastUpdated;
            var yLastUpdated = y.LastUpdated;

            if (xLastUpdated != y.LastUpdated) { return -1 * xLastUpdated.CompareTo(yLastUpdated); }

            return string.CompareOrdinal(x.GetOutputString(), y.GetOutputString());
        }

        private IEnumerable<ICredentialRecord> GetStoredCredentials()
        {
            IEnumerable<ICredentialRecordInfo> recordInfos;
            using (var stream = _streamFactory.GetStream(RecordsLocation))
            {
                recordInfos = _fileStreamer.GetObjectFromStream(stream);
            }

            return recordInfos.Select(r => new CredentialRecord(r, _loggerFactory.GetLogger(typeof(CredentialRecord))));

        }

        private IEnumerable<T> LockAndDo<T>(Func<IEnumerable<T>> action)
        {
            var ownMutex = false;
            try
            {
                ownMutex = _recordsLock.WaitOne();
                if (ownMutex) { return action(); }
            }
            finally
            {
                if (ownMutex) { _recordsLock.ReleaseMutex(); }
            }

            return Enumerable.Empty<T>();
        }

        public void Dispose() => _recordsLock.Dispose();
    }
}