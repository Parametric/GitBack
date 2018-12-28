using System;
using System.Collections.Generic;

namespace GitBack.Credential.Manager
{
    public interface ICredentialRecordsManager : IDisposable
    {
        void EraseRecords(ICredentialRecord match);
        string GetRecord(ICredentialRecord match);
        IEnumerable<string> ListRecords(ICredentialRecord match);
        void StoreRecord(ICredentialRecord record);
    }
}