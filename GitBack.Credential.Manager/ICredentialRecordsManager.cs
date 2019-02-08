using System;
using System.Collections.Generic;
using System.IO;

namespace GitBack.Credential.Manager
{
    public interface ICredentialRecordsManager : IDisposable
    {
        FileInfo RecordsLocation { get; set; }

        void EraseRecords(ICredentialRecord match);
        string GetRecord(ICredentialRecord match);
        IEnumerable<string> ListRecords(ICredentialRecord match);
        void StoreRecord(ICredentialRecord record);
        ICredentialRecord GetCredentialRecordFromOptions(CredentialHelperOptions credentialHelperOptions);
    }
}