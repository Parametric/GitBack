using System;
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

            if (!string.IsNullOrEmpty(Host) && !Host.Equals(record.Host, StringComparison.OrdinalIgnoreCase)) { return false; }

            if (!string.IsNullOrEmpty(Username) && !Username.Equals(record.Username, StringComparison.Ordinal)) { return false; }

            if (!string.IsNullOrEmpty(Path) && !Path.Equals(record.Path, StringComparison.Ordinal)) { return false; }

            return true;
        }

        public bool Equals(ICredentialRecordInfo other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            if (other is null) { return false; }

            if (IsPasswordEncrypted != other.IsPasswordEncrypted || !string.IsNullOrEmpty(Password) && !string.Equals(Password, other.Password, StringComparison.Ordinal))
            {
                return false;
            }

            return IsMatch(other) && other.IsMatch(this);
        }

        public override bool Equals(object other) => other is ICredentialRecordInfo recordInfo && Equals(recordInfo);

        public override int GetHashCode() => IsEmpty() ? 0 : ToString().GetHashCode();

        public override string ToString()
        {
            bool HasValue(string value) => !string.IsNullOrEmpty(value);

            var stringBuilder = new StringBuilder($"{LastUpdated.ToLocalTime()}: ");

            if (HasValue(Username)) { stringBuilder.Append($"-u {Username} "); }

            if (HasValue(Password)) { stringBuilder.Append($"-p {Password} "); }

            if (HasValue(Protocol)) { stringBuilder.Append($"-> {Protocol} "); }

            if (HasValue(Host)) { stringBuilder.Append($"-h {Host} "); }

            if (HasValue(Path)) { stringBuilder.Append($"-\\ {Path} "); }

            stringBuilder.Length--;

            return stringBuilder.ToString();
        }
    }
}