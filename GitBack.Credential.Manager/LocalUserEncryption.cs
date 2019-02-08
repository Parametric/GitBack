using System;
using System.Security.Cryptography;
using System.Text;

namespace GitBack.Credential.Manager
{
    public class LocalUserEncryption : Encryption
    {
        private static readonly Encoding Encoding = new UTF8Encoding(false);
        private static readonly byte[] Entropy = Encoding.GetBytes("GitBack_Protected_Text");

        public override string Encrypt(string clearData)
        {
            if (string.IsNullOrEmpty(clearData)) { return clearData; }

            var clearBytes = Encoding.GetBytes(clearData);
            var protectedBytes = ProtectedData.Protect(clearBytes, Entropy, DataProtectionScope.CurrentUser);
            var protectedText = Convert.ToBase64String(protectedBytes);
            return protectedText;
        }

        public override string Decrypt(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData)) { return encryptedData; }

            var protectedBytes = Convert.FromBase64String(encryptedData);
            var clearBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
            var clearText = Encoding.GetString(clearBytes);
            return clearText;
        }
    }
}