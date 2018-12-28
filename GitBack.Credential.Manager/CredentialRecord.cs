using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GitBack.Credential.Manager
{
    public class CredentialRecordInfo : ICredentialRecordInfo
    {
        public static CredentialRecordInfo Empty => new CredentialRecordInfo();

        public string Protocol { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsPasswordEncrypted { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public void Updated() => LastUpdated = DateTimeOffset.Now;

        public bool IsEmpty()
            => string.IsNullOrEmpty(Protocol) &&
               string.IsNullOrEmpty(Host) &&
               string.IsNullOrEmpty(Username) &&
               string.IsNullOrEmpty(Path);

        public bool IsMatch(ICredentialRecordInfo record)
        {
            // this class matches a record if all the properties' values of this class, except password, url, and LastUpdate, are equal to the same properties' values of the given record
            // Note this class may may match a record that contains properties this class doesn't have
            // so: a.IsMatch.b may not equal b.IsMatch.a
            if (record == null) { return false; }

            if (!string.IsNullOrEmpty(Protocol) && !Protocol.Equals(record.Protocol, StringComparison.OrdinalIgnoreCase)) { return false; }

            if (!string.IsNullOrEmpty(Host) && Host.Equals(record.Host, StringComparison.OrdinalIgnoreCase)) { return false; }

            if (!string.IsNullOrEmpty(Username) && !Username.Equals(record.Username, StringComparison.Ordinal)) { return false; }

            if (!string.IsNullOrEmpty(Path) && !Path.Equals(record.Path, StringComparison.Ordinal)) { return false; }

            return true;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Protocol)) { builder.AppendLine($"{nameof(Protocol)}={Protocol}"); }

            if (!string.IsNullOrEmpty(Host)) { builder.AppendLine($"{nameof(Host)}={Host}"); }

            if (!string.IsNullOrEmpty(Username)) { builder.AppendLine($"{nameof(Username)}={Username}"); }
            if (!string.IsNullOrEmpty(Password)) { builder.AppendLine($"{nameof(Password)}={Password}"); }

            if (!string.IsNullOrEmpty(Path)) { builder.AppendLine($"{nameof(Path)}={Path}"); }

            return builder.ToString();
        }
    }

    public class CredentialRecord : ICredentialRecord
    {
        private readonly ICredentialRecordInfo _credentialRecordInfo;

        public CredentialRecord() : this(CredentialRecordInfo.Empty) { }

        public CredentialRecord(ICredentialRecordInfo credentialRecordInfo) => _credentialRecordInfo = credentialRecordInfo;

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

        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(Protocol)) { return null; }

                // note: what git calls "Host", .Net calls "Authority"
                var uriBuilder = !string.IsNullOrEmpty(Host)
                    ? new UriBuilder(Protocol + Uri.SchemeDelimiter + Host)
                    : new UriBuilder { Scheme = Protocol };
                uriBuilder.UserName = Username;
                uriBuilder.Password = Password;

                try
                {
                    var uri = uriBuilder.Uri;

                    if (!string.IsNullOrEmpty(Path)) { uri = new Uri(uri, Path); }

                    return uri.AbsoluteUri;
                }
                catch (FormatException)
                {
                    // note add logging
                    return null;
                }
            }
            set
            {
                try
                {
                    var uri = new Uri(value, UriKind.Absolute);
                    var uriBuilder = new UriBuilder(uri.ToString());
                    Protocol = uri.Scheme;
                    Username = uriBuilder.UserName;
                    Password = uriBuilder.Password;
                    // note: what git calls "Host", .Net calls "Authority"
                    Host = uri.Authority;
                    Path = uri.AbsolutePath + uri.Query + uri.Fragment;
                    Path = Path.TrimStart('/');
                }
                catch (FormatException)
                {
                    // note add logging
                }
            }
        }

        public bool IsEmpty()
            => string.IsNullOrEmpty(Protocol) &&
               string.IsNullOrEmpty(Host) &&
               string.IsNullOrEmpty(Username) &&
               string.IsNullOrEmpty(Path);

        public DateTimeOffset LastUpdated => _credentialRecordInfo.LastUpdated;

        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        private static readonly SortedDictionary<string, PropertyInfo> _propertiesNames = new SortedDictionary<string, PropertyInfo>(Comparer);
        private static IDictionary<string, PropertyInfo> PropertiesNames
        {
            get
            {
                if (!_propertiesNames.IsEmpty()) { return _propertiesNames; }

                var myType = typeof(CredentialRecord);
                var publicProperties = myType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                             .Where(x => x.GetMethod != null && x.GetMethod.IsPublic && x.SetMethod != null && x.SetMethod.IsPublic);

                foreach (var publicProperty in publicProperties)
                {
                    var name = publicProperty.Name;

                    if (!_propertiesNames.ContainsKey(name))
                    {
                        _propertiesNames.Add(name, publicProperty);
                    }
                }

                return _propertiesNames;
            }
        }

        public void AddOrUpdatePropertyValue(string propertyName, string value)
        {
            PropertyInfo property = null;
            if (!PropertiesNames.ContainsKey(propertyName))
            {
                // log something?
                return;
            }

            property = PropertiesNames[propertyName];

            property.SetValue(this, value);
        }

        public ICredentialRecordInfo GetCredentialRecordInfo() => _credentialRecordInfo;

        public bool IsMatch(ICredentialRecord record) => _credentialRecordInfo.IsMatch(record?.GetCredentialRecordInfo());

        public override string ToString() => _credentialRecordInfo.ToString();
    }
}