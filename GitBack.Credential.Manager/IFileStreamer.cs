using System.Collections.Generic;
using System.IO;

namespace GitBack.Credential.Manager
{
    public interface IFileStreamer
    {
        IEnumerable<ICredentialRecordInfo> GetObjectFromStream(Stream stream);
        void StoreObjectToStream(IEnumerable<ICredentialRecordInfo> objectToStore, Stream stream);
    }
}