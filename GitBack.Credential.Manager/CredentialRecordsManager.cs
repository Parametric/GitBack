using System;
using System.Collections.Generic;
using System.Linq;

namespace GitBack.Credential.Manager {
    public class CredentialRecordsManager : ICredentialRecordsManager
    {
        private readonly IFileStreamer<List<ICredentialRecord>> _fileStreamer;
        private readonly string _recordsLocation;
        private readonly IMutex _recordsLock;
        private readonly IEncryption _encryption;

        public CredentialRecordsManager(IFileStreamer<List<ICredentialRecord>> fileStreamer, string recordLocation, IMutex recordsLock, IEncryption encryption)
        {
            _fileStreamer = fileStreamer;
            _recordsLocation = recordLocation;
            _recordsLock = recordsLock;
            _encryption = encryption;
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
            var records = ListRecords(match);
            var record = records.FirstOrDefault() ?? string.Empty;
            return record;
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
            _fileStreamer.StoreObjectOfType(recordsToSave, _recordsLocation);
        }

        private static int CompareRecordsByLastUpdate(ICredentialRecord x, ICredentialRecord y)
        {
            var xLastUpdated = x.LastUpdated;
            var yLastUpdated = y.LastUpdated;

            return xLastUpdated != y.LastUpdated
                ? yLastUpdated.CompareTo(xLastUpdated)
                : string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
        }

        private IEnumerable<ICredentialRecord> GetStoredCredentials() => _fileStreamer.GetObjectOfType(_recordsLocation);

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