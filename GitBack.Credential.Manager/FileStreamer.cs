using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ExtendedXmlSerializer.ExtensionModel.Xml;

namespace GitBack.Credential.Manager
{
    public class FileStreamer : IFileStreamer
    {
        private readonly IExtendedXmlSerializer _serializer;
        private readonly XmlWriterSettings _writerSettings;
        private readonly XmlReaderSettings _readerSettings;

        public FileStreamer(IExtendedXmlSerializer serializer, XmlWriterSettings writerSettings, XmlReaderSettings readerSettings)
        {
            _serializer = serializer;
            _writerSettings = writerSettings;
            _readerSettings = readerSettings;
        }

        public IEnumerable<ICredentialRecordInfo> GetObjectFromStream(Stream stream)
        {
            if (stream.CanSeek && stream.Position >= stream.Length - 1)
            {
                return Enumerable.Empty<ICredentialRecordInfo>();
            }

            var records = _serializer.Deserialize<IEnumerable<ICredentialRecordInfo>>(_readerSettings, stream);

            if (records is ICollection<ICredentialRecordInfo> recordCollection) { return recordCollection; }

            return records.ToList();
        }

        public void StoreObjectToStream(IEnumerable<ICredentialRecordInfo> objectToStore, Stream stream)
        {
            var recordCollection = objectToStore != null
                ? objectToStore as ICollection<ICredentialRecordInfo> ?? objectToStore.ToList()
                : Enumerable.Empty<ICredentialRecordInfo>();

            _serializer.Serialize(_writerSettings, stream, recordCollection);
        }
    }
}