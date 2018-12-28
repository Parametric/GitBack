using System;

namespace GitBack.Credential.Manager
{
    public interface ICredentialRecord : ICredentialRecordCommon<ICredentialRecord>
    {
        string Url { get; set; }

        DateTimeOffset LastUpdated { get; }

        void AddOrUpdatePropertyValue(string propertyName, string value);

        ICredentialRecordInfo GetCredentialRecordInfo();
    }
}