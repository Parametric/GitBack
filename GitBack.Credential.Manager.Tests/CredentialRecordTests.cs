using System;
using System.Collections.Generic;
using System.Text;
using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ExtensionModel.Xml;
using NSubstitute;
using NUnit.Framework;

namespace GitBack.Credential.Manager.Tests
{
    public class CredentialRecordTests
    {
        private ILogger _logger;

        [SetUp]
        public void BeforeEach()
        {
            var xmlContainer = new ConfigurationContainer();
            var encryptionAlgorithm = new LocalUserEncryption();

            _logger = Substitute.For<ILogger>();
        }

        [TestCaseSource(nameof(UrlTestData), new object[] { "Sets Urls" })]
        [Test]
        public void CredentialRecord_SetsUrls(UrlTestCaseData urlData, CredentialRecord expectedRecord)
        {
            var subject = new CredentialRecord(_logger) { Url = urlData.GivenUrl };

            Assert.Multiple(() =>
            {
                Assert.That(subject.Url, Is.EqualTo(urlData.ExpectedUrl));
                Assert.That(subject, Is.EqualTo(expectedRecord));
            });
        }

        public static IEnumerable<TestCaseData> UrlTestData(string namePrefix)
        {
            var logger = Substitute.For<ILogger>();
            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"Https://example.com/", ExpectedUrl = "https://example.com/" },
                new CredentialRecord(logger) { Protocol = "https", Host = "example.com" }
            ).SetName($"{namePrefix}: sets url w/ Https Protocol");

            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"C:\example\path", ExpectedUrl = "file:///C:/example/path" },
                new CredentialRecord(logger) { Protocol = "file", Path = "C:/example/path" }
            ).SetName($"{namePrefix}: sets url w/ file Protocol");

            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"\\Remote.Host\example\path", ExpectedUrl = "file://remote.host/example/path" },
                new CredentialRecord(logger) { Protocol = "file", Host = "remote.host", Path = "example/path" }
            ).SetName($"{namePrefix}: sets url w/ remote file Protocol");

            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"https://example.com/path" },
                new CredentialRecord(logger) { Protocol = "https", Host = "example.com", Path = "path" }
            ).SetName($"{namePrefix}: sets url w/ path");

            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"https://example.com:82/path" },
                new CredentialRecord(logger) { Protocol = "https", Host = "example.com:82", Path = "path" }
            ).SetName($"{namePrefix}: sets url w/ path and port");
            
            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"https://user:password@example.com/"}, 
                new CredentialRecord(logger) { Protocol = "https", Host = "example.com", Username = "user", Password = "password" }
            ).SetName($"{namePrefix}: sets url w/ user and password");
            
            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"https://user:password@example.com/Path" },
                new CredentialRecord(logger) { Protocol = "https", Host = "example.com", Username = "user", Password = "password", Path = "Path" }
            ).SetName($"{namePrefix}: sets url w/ user, password and path");
            
            yield return new TestCaseData(
                new UrlTestCaseData { GivenUrl = @"https://user:invalid|example.com/Path", ExpectedUrl = null },
                new CredentialRecord(logger)
            ).SetName("CredentialRecord does not set invalid url");
        }

        public class UrlTestCaseData
        {
            public string GivenUrl { get; set; }

            private string _expectedUrl;
            private bool _expectedUrlSet;

            public override string ToString() => ExpectedUrl == null ? "Expecting Url=null" : $"Expecting Url={ExpectedUrl}";

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

        [TestCaseSource(nameof(PropertyValueTestData), new object[] { "AddOrUpdatePropertyValue" })]
        [Test]
        public void AddOrUpdatePropertyValue(string propertyName, string value, bool expectSubjectModified)
        {
            var subject = new CredentialRecord(_logger);
            subject.AddOrUpdatePropertyValue(propertyName, value);

            if (!expectSubjectModified)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(subject.IsEmpty);
                    Assert.That(subject.ToString(), Is.EqualTo(new CredentialRecord(_logger).ToString()));
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

        public static IEnumerable<TestCaseData> PropertyValueTestData(string namePrefix)
        {
            yield return new TestCaseData(nameof(ICredentialRecord.Protocol), "foo", true).SetName($"{namePrefix}: Add Or Update Protocol");
            yield return new TestCaseData(nameof(ICredentialRecord.Host), "example.com", true).SetName($"{namePrefix}: Add Or Update Host");
            yield return new TestCaseData(nameof(ICredentialRecord.Password), "pass", true).SetName($"{namePrefix}: Add Or Update Password");
            yield return new TestCaseData(nameof(ICredentialRecord.Path), "myPath", true).SetName($"{namePrefix}: Add Or Update Path");
            yield return new TestCaseData(nameof(ICredentialRecord.Url), "https://example.com/", true).SetName($"{namePrefix}: Add Or Update Url");
            yield return new TestCaseData(nameof(ICredentialRecord.Username), "user", true).SetName($"{namePrefix}: Add Or Update Username");
            yield return new TestCaseData("InvalidProperty", null, false).SetName($"{namePrefix}: Don't Add Or Update InvalidProperty");
        }

        [TestCaseSource(nameof(ToStringTestData), new object[] { "GetOutputString" })]
        [Test]
        public void CredentialRecord_GetOutputString(CredentialRecord record)
        {
            var expectedResult = new StringBuilder();
            if (!string.IsNullOrEmpty(record.Protocol)) { expectedResult.AppendLine($"{nameof(ICredentialRecord.Protocol)}={record.Protocol}"); }
            if (!string.IsNullOrEmpty(record.Host)) { expectedResult.AppendLine($"{nameof(ICredentialRecord.Host)}={record.Host}"); }
            if (!string.IsNullOrEmpty(record.Username)) { expectedResult.AppendLine($"{nameof(ICredentialRecord.Username)}={record.Username}"); }
            if (!string.IsNullOrEmpty(record.Password)) { expectedResult.AppendLine($"{nameof(ICredentialRecord.Password)}={record.Password}"); }
            if (!string.IsNullOrEmpty(record.Path)) { expectedResult.AppendLine($"{nameof(ICredentialRecord.Path)}={record.Path}"); }


            var stringValue = record.GetOutputString();

            Assert.That(stringValue, Is.EqualTo(expectedResult.ToString()));
        }

        [TestCaseSource(nameof(ToStringTestData), new object[] { "ToString" })]
        [Test]
        public void CredentialRecord_ToString(CredentialRecord record)
        {
            var stringValue = record.ToString();

            Assert.Multiple(() =>
            {
                if (!string.IsNullOrEmpty(record.Protocol)) { Assert.That(stringValue, Contains.Substring(record.Protocol).IgnoreCase); }
                if (!string.IsNullOrEmpty(record.Host)) { Assert.That(stringValue, Contains.Substring(record.Host).IgnoreCase); }
                if (!string.IsNullOrEmpty(record.Username)) { Assert.That(stringValue, Contains.Substring(record.Username)); }
                if (!string.IsNullOrEmpty(record.Password)) { Assert.That(stringValue, Contains.Substring(record.Password)); }
                if (!string.IsNullOrEmpty(record.Path)) { Assert.That(stringValue, Contains.Substring(record.Path).IgnoreCase); }
            });
        }

        public static IEnumerable<TestCaseData> ToStringTestData(string namePrefix)
        {
            var logger = Substitute.For<ILogger>();
            yield return new TestCaseData(new CredentialRecord(logger) { Host = "Host", Protocol = "https", Username = "user", Password = "Password", Path = "path"}).SetName($"{namePrefix}: standard");
            yield return new TestCaseData(new CredentialRecord(logger) { Host = "Host", Protocol = "https", Username = "us@er", Password = "Password", Path = "path"}).SetName($"{namePrefix}: username: us@er");
            yield return new TestCaseData(new CredentialRecord(logger) { Host = "Host", Protocol = "https", Username = "user", Password = "P@ssword", Path = "path"}).SetName($"{namePrefix}: password: p@ssword");
            yield return new TestCaseData(new CredentialRecord(logger) { Host = "Host", Username = "user", Password = "Password", Path = "path"}).SetName($"{namePrefix}: no protocol");
            yield return new TestCaseData(new CredentialRecord(logger) { Host = "Host", Protocol = "file", Username = "user", Password = "Password", Path = "path"}).SetName($"{namePrefix}: protocol: file");
            yield return new TestCaseData(new CredentialRecord(logger) { Protocol = "file", Username = "user", Password = "Password", Path = "path"}).SetName($"{namePrefix}: protocol: file, no host");
        }
    }
}
