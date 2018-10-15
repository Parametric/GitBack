using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GitBack
{
    public sealed class ConfigProperties : IEquatable<ConfigProperties>
    {
        private static readonly Encoding Utf8 = new UTF8Encoding();
        private static readonly byte[] Salt = Guid.Parse("23a0b616-9923-4878-9bc8-0f55da30955a").ToByteArray();

        public string Username { get; set; }
        public string Token { get; set; }
        public string Organization { get; set; }
        public string BackupLocation { get; set; }
        public string ProjectFilter { get; set; }

        private ConfigProperties()
        {
        }

        public ConfigProperties(string username) : this()
        {
            if (username == null)
            {
                return;
            }
            Username = username;
        }

        private ConfigProperties(ProgramOptions options) : this()
        {
            if (options == null)
            {
                return;
            }
            Username = options.Username;
            Organization = options.Organization;
            ProjectFilter = options.ProjectFilter;

            BackupLocation = options?.BackupLocation?.FullName;
            Token = EncryptToken(options.Token);
        }

        private static string EncryptToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
 
            try
            {
                var tokenBytes = Utf8.GetBytes(token);
                var encryptedData = ProtectedData.Protect(tokenBytes, Salt, DataProtectionScope.CurrentUser);
                var encryptedString = Convert.ToBase64String(encryptedData);
                return encryptedString;
            }
            catch (Exception e) when (e is CryptographicException || e is NotSupportedException)
            {
                throw new ApplicationException("Could not encrypt token to store in config", e);
            }
        }
 
        private static string DecryptToken(string encryptedString)
        {
            if (string.IsNullOrEmpty(encryptedString))
            {
                return null;
            }
 
            try
            {
                var encryptedData = Convert.FromBase64String(encryptedString);
                var decryptedData = ProtectedData.Unprotect(encryptedData, Salt, DataProtectionScope.CurrentUser);
                return Utf8.GetString(decryptedData);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Failed to decrypt Token. You may have to save an new one.", e);
            }
        }

        public static implicit operator ConfigProperties(ProgramOptions options) => new ConfigProperties(options);
 
        public static implicit operator ProgramOptions(ConfigProperties properties) => ConvertToProgramOptions(properties);

        private static ProgramOptions ConvertToProgramOptions(ConfigProperties properties)
        {
            if (properties == null)
            {
                return null;
            }

            return new ProgramOptions
            {
                Username = properties.Username,
                Organization = properties.Organization,
                ProjectFilter = properties.ProjectFilter,

                BackupLocation = properties.BackupLocation == null?  null : new DirectoryInfo(properties.BackupLocation),
                Token = DecryptToken(properties.Token)
            };

        }

        public bool Equals(ConfigProperties other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Username, other.Username, StringComparison.InvariantCultureIgnoreCase) && 
                   string.Equals(Token, other.Token, StringComparison.InvariantCulture) && 
                   string.Equals(Organization, other.Organization, StringComparison.InvariantCultureIgnoreCase) && 
                   string.Equals(BackupLocation, other.BackupLocation, StringComparison.InvariantCultureIgnoreCase) && 
                   string.Equals(ProjectFilter, other.ProjectFilter, StringComparison.InvariantCulture);
        }

        public override bool Equals(object obj) => !(obj is null) && (obj is ConfigProperties other && Equals(other));

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Username != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Username) : 0);
                hashCode = (hashCode * 397) ^ (Token != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Token) : 0);
                hashCode = (hashCode * 397) ^ (Organization != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Organization) : 0);
                hashCode = (hashCode * 397) ^ (BackupLocation != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(BackupLocation) : 0);
                hashCode = (hashCode * 397) ^ (ProjectFilter != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(ProjectFilter) : 0);
                return hashCode;
            }
        }
    }
}