using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GitBack.Credential.Manager
{
    public class CredentialRecord : ICredentialRecord
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        private static readonly IReadOnlyDictionary<string, PropertyInfo> PropertiesNames;

        static CredentialRecord()
        {
            var myType = typeof(CredentialRecord);
            var publicProperties = myType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                         .Where(x => x.GetMethod != null && x.GetMethod.IsPublic && x.SetMethod != null && x.SetMethod.IsPublic);


            PropertiesNames = publicProperties.ToImmutableSortedDictionary(k => k.Name, v => v, Comparer);
        }


        private readonly ICredentialRecordInfo _credentialRecordInfo;
        private readonly ILogger _logger;

        public CredentialRecord(ILogger logger) : this(CredentialRecordInfo.Empty, logger) { }

        public CredentialRecord(ICredentialRecordInfo credentialRecordInfo, ILogger logger)
        {
            _credentialRecordInfo = credentialRecordInfo;
            _logger = logger;
        }

        public string Protocol
        {
            get => _credentialRecordInfo.Protocol;
            set
            {
                _credentialRecordInfo.Protocol = value;
                _credentialRecordInfo.Updated();
            }
        }

        public string Host
        {
            get => _credentialRecordInfo.Host;
            set
            {
                _credentialRecordInfo.Host = value;
                _credentialRecordInfo.Updated();
            }
        }
        public string Path
        {
            get => _credentialRecordInfo.Path;
            set
            {
                _credentialRecordInfo.Path = value;
                _credentialRecordInfo.Updated();
            }
        }
        public string Username
        {
            get => _credentialRecordInfo.Username;
            set
            {
                _credentialRecordInfo.Username = value;
                _credentialRecordInfo.Updated();
            }
        }
        public string Password
        {
            get => _credentialRecordInfo.Password;
            set
            {
                _credentialRecordInfo.Password = value;
                _credentialRecordInfo.Updated();
            }
        }

        private static Uri CreateUri(string protocol, string hostOrPath, string path = null)
        {
            var uriString = $"{protocol}{Uri.SchemeDelimiter}{hostOrPath}";
            var uri = Uri.TryCreate(uriString, UriKind.Absolute, out var uriResult) ? uriResult : null;
            if (path != null)
            {
                uri = Uri.TryCreate(uri, path, out var pathResult) ? pathResult : null;
            }

            return uri;
        }

        private string SimpleUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Protocol)) { return null; }

                // What git (and this class) call host and what .Net calls Authority is actually host[:port]
                // they are both wrong
                // from: https://en.wikipedia.org/wiki/URL#Syntax
                // URI = scheme:[//authority]path[?query][#fragment]
                // authority = [userinfo@]host[:port]
                // userinfo = username[:password]
                
                Uri uri = null;
                if (!string.IsNullOrEmpty(Host))
                {
                    uri = CreateUri(Protocol, Host, Path);
                }
                else if (!string.IsNullOrEmpty(Path))
                {
                    uri = CreateUri(Protocol, Path);
                }

                return uri?.AbsoluteUri;
            }
        }

        public string Url
        {
            get
            {
                var simpleUrl = SimpleUrl;
                if (simpleUrl == null) { return null; }

                if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)) { return simpleUrl; }

                if (string.IsNullOrEmpty(Username)) { return null; }

                var uriBuilder = new UriBuilder(simpleUrl) { UserName = Username, Password = Password };

                return Uri.TryCreate(uriBuilder.ToString(), UriKind.Absolute, out var uri) ? uri.AbsoluteUri : null;
            }
            set
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value)) { return; }

                    var uri = new Uri(value, UriKind.Absolute);
                    var uriBuilder = new UriBuilder(uri.ToString());
                    Protocol = uri.Scheme;
                    if (!string.IsNullOrEmpty(uriBuilder.UserName)) { Username = uriBuilder.UserName; }
                    if (!string.IsNullOrEmpty(uriBuilder.Password)) { Password = uriBuilder.Password; }

                    // note: what git calls "Host", .Net calls "Authority" see note in SimpleUrl
                    Host = uri.Authority;
                    Path = uri.AbsolutePath + uri.Query + uri.Fragment;
                    Path = Path.TrimStart('/');
                    Console.Error.WriteLine($"In Set URL: {this.ToString()}/n{this.GetOutputString()}");
                }
                catch (FormatException e) { _logger.Warn($"Could not set string to Uri", e); }
            }
        }

        public bool IsEmpty()
            => string.IsNullOrEmpty(Protocol) &&
               string.IsNullOrEmpty(Host) &&
               string.IsNullOrEmpty(Username) &&
               string.IsNullOrEmpty(Path);

        public DateTimeOffset LastUpdated => _credentialRecordInfo.LastUpdated;

        public void AddOrUpdatePropertyValue(string propertyName, string value)
        {
            if (!PropertiesNames.ContainsKey(propertyName))
            {
                _logger.Warn($"propertyName: {propertyName} is not a valid property name");
                return;
            }

            var property = PropertiesNames[propertyName];

           property?.SetValue(this, value);
        }

        public ICredentialRecordInfo GetCredentialRecordInfo() => _credentialRecordInfo;

        public bool IsMatch(ICredentialRecord record) => _credentialRecordInfo.IsMatch(record?.GetCredentialRecordInfo());

        public bool Equals(ICredentialRecord other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            if (other is null) { return false; }

            var otherRecordInfo = other.GetCredentialRecordInfo();
            if (ReferenceEquals(_credentialRecordInfo, otherRecordInfo)) { return true; }

            if (otherRecordInfo is null) { return false; }

            return _credentialRecordInfo.Equals(otherRecordInfo);
        }

        public override bool Equals(object other) => other is ICredentialRecord recordInfo && Equals(recordInfo);

        public override int GetHashCode() => IsEmpty() ? 0 : _credentialRecordInfo.GetHashCode();

        public string GetOutputString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Protocol)) { builder.AppendLine($"{nameof(Protocol)}={Protocol}"); }

            if (!string.IsNullOrEmpty(Host)) { builder.AppendLine($"{nameof(Host)}={Host}"); }

            if (!string.IsNullOrEmpty(Username)) { builder.AppendLine($"{nameof(Username)}={Username}"); }
            if (!string.IsNullOrEmpty(Password)) { builder.AppendLine($"{nameof(Password)}={Password}"); }

            if (!string.IsNullOrEmpty(Path)) { builder.AppendLine($"{nameof(Path)}={Path}"); }

            return builder.ToString();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder($"{LastUpdated.ToLocalTime()}: ");

            if (IsEmpty())
            {
                stringBuilder.Append(nameof(IsEmpty));
                return stringBuilder.ToString();
            }

            var url = Url;
            if (url != null)
            {
                stringBuilder.Append("-: ").Append(url);
                return stringBuilder.ToString();
            }

            url = SimpleUrl;
            if (url != null)
            {
                stringBuilder.Append("-: ").Append(url).Append(" ");
                if (!string.IsNullOrEmpty(Username)) { stringBuilder.Append($"-u {Username} "); }
                if (!string.IsNullOrEmpty(Password)) { stringBuilder.Append($"-p {Password} "); }

                return stringBuilder.ToString();
            }


            return _credentialRecordInfo.ToString();
        }
    }
}