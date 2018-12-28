using System;

namespace GitBack.Credential.Manager
{
    public interface ICredentialRecordInfo : ICredentialRecordCommon<ICredentialRecordInfo>
    {
        bool IsPasswordEncrypted { get; set; }

        DateTimeOffset LastUpdated { get; set; }

        void Updated();
    }
}