using System;
using System.IO;
using System.Xml;
using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ExtensionModel.Xml;
using NUnit.Framework;

namespace GitBack.Credential.Manager.Tests
{
    public class CredentialRecordInfoTests
    {
        private IExtendedXmlSerializer _serializer;

        [SetUp]
        public void BeforeEach()
        {
            var xmlContainer = new ConfigurationContainer();
            _serializer = xmlContainer.UseOptimizedNamespaces()
                                      .UseAutoFormatting()
                                      .Create();
        }

        [Test]
        public void XmlSerialize_RoundTrip()
        {
            var record = new CredentialRecordInfo()
            {
                Host = "Host", Protocol = "Https", Username = "user", Password = "Password"
            };
            var lastUpdate = record.LastUpdated;


            var fileStream = new MemoryStream();
            var xmlWriter = XmlWriter.Create(fileStream);

            // Act
            _serializer.Serialize(xmlWriter, record);
            xmlWriter.Flush();
            fileStream.Position = 0;
            var reader = new StreamReader(fileStream);
            var read = reader.ReadToEnd();
            Console.WriteLine(read);

            fileStream.Position = 0;

            var stringReader = new StringReader(read);
            var deserializedRecord = _serializer.Deserialize<ICredentialRecordInfo>(stringReader);

            // assert
            Assert.That(deserializedRecord, Is.Not.Null);
            Assert.That(deserializedRecord.IsEmpty(), Is.False);
            Assert.That(deserializedRecord, Is.EqualTo(record));
            Assert.That(deserializedRecord.LastUpdated, Is.EqualTo(lastUpdate));
        }
    }
}
