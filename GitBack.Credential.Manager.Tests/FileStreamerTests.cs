using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ExtendedXmlSerializer.ExtensionModel.Xml;
using Ninject;
using NUnit.Framework;

namespace GitBack.Credential.Manager.Tests
{
    public class FileStreamerTests
    {
        private static IKernel _kernel;
        private IExtendedXmlSerializer _serializer;
        private XmlWriterSettings _xmlWriterSettings;
        private XmlReaderSettings _xmlReaderSettings;

        [OneTimeSetUp]
        public static void BeforeAll()
        {
            if (_kernel == null)
            {
                Bootstrapper.ConfigureBindings();
                _kernel = Bootstrapper.Kernel;
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            _serializer = _kernel.Get<IExtendedXmlSerializer>();
            _xmlWriterSettings = _kernel.Get<XmlWriterSettings>();
            _xmlReaderSettings = _kernel.Get<XmlReaderSettings>();
        }

        [Test]
        public void GetObjectFromStream_EmptyList()
        {
            var subject = new FileStreamer(_serializer, _xmlWriterSettings, _xmlReaderSettings);
            var stream = new MemoryStream();
            var objects = new List<CredentialRecordInfo>();

            _serializer.Serialize(_xmlWriterSettings, stream, objects);
            stream.Seek(0, SeekOrigin.Begin);

            var actualObjects = subject.GetObjectFromStream(stream);

            Assert.That(actualObjects, Is.Empty);
        }

        [Test]
        public void ListRecords_NoRecordsFile()
        {
            var subject = new FileStreamer(_serializer, _xmlWriterSettings, _xmlReaderSettings);
            var stream = new MemoryStream();
            var result = subject.GetObjectFromStream(stream);

            Assert.That(result, Is.EquivalentTo(Enumerable.Empty<ICredentialRecord>()));
        }


        [Test]
        public void GetObjectFromStream()
        {

            var subject = new FileStreamer(_serializer, _xmlWriterSettings, _xmlReaderSettings);
            var stream = new MemoryStream();
            var objects = new List<CredentialRecordInfo>
            {
                new CredentialRecordInfo { Host = "example.com" },
                new CredentialRecordInfo { Username = "foo", Password = "Password"},
                new CredentialRecordInfo { Protocol = "https", Host = "example.com", Path = "1234" },
            };

            _serializer.Serialize(_xmlWriterSettings, stream, objects);
            stream.Seek(0, SeekOrigin.Begin);

            var actualObjects = subject.GetObjectFromStream(stream);

            Assert.That(actualObjects, Is.EquivalentTo(objects));
        }

        [Test]
        public void RoundTrip_EmptyList()
        {
            var subject = new FileStreamer(_serializer, _xmlWriterSettings, _xmlReaderSettings);
            var stream = new MemoryStream();
            var objects = new List<CredentialRecordInfo>();

            subject.StoreObjectToStream(objects, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var actualObjects = subject.GetObjectFromStream(stream);

            Assert.That(actualObjects, Is.Empty);

        }

        [Test]
        public void RoundTrip()
        {
            var subject = new FileStreamer(_serializer, _xmlWriterSettings, _xmlReaderSettings);
            var stream = new MemoryStream();
            var objects = new List<CredentialRecordInfo>
            {
                new CredentialRecordInfo { Host = "example.com" },
                new CredentialRecordInfo { Username = "foo", Password = "Password"},
                new CredentialRecordInfo { Protocol = "https", Host = "example.com", Path = "1234" },
            };

            subject.StoreObjectToStream(objects, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var actualObjects = subject.GetObjectFromStream(stream);

            Assert.That(actualObjects, Is.EquivalentTo(objects));
        }

    }
}
