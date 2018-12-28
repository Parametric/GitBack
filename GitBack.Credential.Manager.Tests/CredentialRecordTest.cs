using System;
using System.Collections.Generic;
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
            var encryptionAlgorithm = new LocalUserEncryption();
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
            //var reader = XmlReader.Create(fileStream);
            var stringreader = new StringReader(read);
            var deserializedRecord = _serializer.Deserialize<ICredentialRecordInfo>(stringreader);

            // assert
            Assert.That(deserializedRecord, Is.Not.Null);
            Assert.That(deserializedRecord.IsEmpty(), Is.False);
            Assert.That(deserializedRecord.ToString(), Is.EqualTo(record.ToString()));
            Assert.That(deserializedRecord.LastUpdated, Is.EqualTo(lastUpdate));
        }

        [Test]
        public void CredentialRecordInfo_ToString()
        {
            var record = new CredentialRecordInfo
            {
                Host = "Host", Protocol = "Https", Username = "user", Password = "Password"
            };
            var expectedResult = $"{nameof(ICredentialRecord.Protocol)}={record.Protocol}\r\n"
                               + $"{nameof(ICredentialRecord.Host)}={record.Host}\r\n"
                               + $"{nameof(ICredentialRecord.Username)}={record.Username}\r\n"
                               + $"{nameof(ICredentialRecord.Password)}={record.Password}\r\n";
                               

            var stringValue = record.ToString();

            Assert.That(stringValue, Is.EqualTo(expectedResult));

        }
    }

    public class CredentialRecordTests
    {
        private IExtendedXmlSerializer _serializer;

        [SetUp]
        public void BeforeEach()
        {
            var xmlContainer = new ConfigurationContainer();
            var encryptionAlgorithm = new LocalUserEncryption();
            _serializer = xmlContainer.UseOptimizedNamespaces()
                                      .UseAutoFormatting()
                                      .Create();
        }

        [TestCaseSource(nameof(UrlTestData))]
        [Test]
        public void CredentialRecord_SetsUrls(UrlTestCaseData urlData, CredentialRecord expectedRecord)
        {

            var subject = new CredentialRecord { Url = urlData.GivenUrl };

            Assert.Multiple(() => {
                Assert.That(subject.Url, Is.EqualTo(urlData.ExpectedUrl));
                Assert.That(subject.ToString(), Is.EqualTo(expectedRecord.ToString()));
            });
        }

        public static IEnumerable<object []> UrlTestData()
        {
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"Https://example.com/", ExpectedUrl = "https://example.com/"}, new CredentialRecord { Protocol = "https", Host = "example.com"} };
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"C:\example\path", ExpectedUrl = "file:///C:/example/path"}, new CredentialRecord { Protocol = "file", Path = "C:/example/path" } };
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"https://example.com/path" }, new CredentialRecord { Protocol = "https", Host = "example.com", Path = "path" } };
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"https://example.com:82/path"},  new CredentialRecord { Protocol = "https", Host = "example.com:82", Path = "path" } };
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"https://user:password@example.com/"}, new CredentialRecord { Protocol = "https", Host = "example.com", Username = "user", Password = "password" } };
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"https://user:password@example.com/Path"}, new CredentialRecord { Protocol = "https", Host = "example.com", Username = "user", Password = "password", Path = "Path" } };
            yield return new object[] { new UrlTestCaseData { GivenUrl = @"https://user:invalid|example.com/Path", ExpectedUrl = null}, new CredentialRecord() };
        }

        public class UrlTestCaseData
        {
            public string GivenUrl { get; set; }

            private string _expectedUrl;
            private bool _expectedUrlSet;

            public override string ToString() => $"Expecting Url={ExpectedUrl}";

            public string ExpectedUrl
            {
                get
                {
                    if (!_expectedUrlSet) { _expectedUrl = GivenUrl; }

                    return _expectedUrl;
                }
                set
                {
                    _expectedUrl = value;
                    _expectedUrlSet = true;
                }
            }
        } 

        [TestCaseSource(nameof(PropertyValueTestData))]
        [Test]
        public void AddOrUpdatePropertyValue(string propertyName, string value, bool expectSubjectModified)
        {
            var subject = new CredentialRecord();
            subject.AddOrUpdatePropertyValue(propertyName, value);

            if (!expectSubjectModified)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(subject.IsEmpty);
                    Assert.That(subject.ToString(), Is.EqualTo(new CredentialRecord().ToString()));
                });
                return;
            }

            switch (propertyName)
            {
                case nameof(ICredentialRecord.Protocol):
                    Assert.That(subject.Protocol, Is.EqualTo(value));
                    break;
                case nameof(ICredentialRecord.Host):
                    Assert.That(subject.Host, Is.EqualTo(value));
                    break;
                case nameof(ICredentialRecord.Password):
                    Assert.That(subject.Password, Is.EqualTo(value));
                    break;
                case nameof(ICredentialRecord.Path):
                    Assert.That(subject.Path, Is.EqualTo(value));
                    break;
                case nameof(ICredentialRecord.Url):
                    Assert.That(subject.Url, Is.EqualTo(value));
                    break;
                case nameof(ICredentialRecord.Username):
                    Assert.That(subject.Username, Is.EqualTo(value));
                    break;
                default:
                    Assert.Fail("Unknown case value");
                    break;
            }
        }

        public static IEnumerable<object[]> PropertyValueTestData()
        {
            yield return new object[] { nameof(ICredentialRecord.Protocol), "foo", true};
            yield return new object[] { nameof(ICredentialRecord.Host), "example.com", true };
            yield return new object[] { nameof(ICredentialRecord.Password), "pass", true };
            yield return new object[] { nameof(ICredentialRecord.Path), "myPath", true };
            yield return new object[] { nameof(ICredentialRecord.Url), "https://example.com/", true };
            yield return new object[] { nameof(ICredentialRecord.Username), "user", true };
            yield return new object[] { "InvalidProperty", null, false };
        }

        [Test]
        public void CredentialRecord_ToString()
        {
            var record = new CredentialRecord
            {
                Host = "Host", Protocol = "Https", Username = "user", Password = "Password"
            };
            var expectedResult = $"{nameof(ICredentialRecord.Protocol)}={record.Protocol}\r\n"
                               + $"{nameof(ICredentialRecord.Host)}={record.Host}\r\n"
                               + $"{nameof(ICredentialRecord.Username)}={record.Username}\r\n"
                               + $"{nameof(ICredentialRecord.Password)}={record.Password}\r\n";
                               

            var stringValue = record.ToString();

            Assert.That(stringValue, Is.EqualTo(expectedResult));

        }
    }
}
