namespace GitBack.Credential.Manager 
{
    public interface IEncryption : ExtendedXmlSerializer.ExtensionModel.Encryption.IEncryption
    {
        string Encrypt(string clearData);

        string Decrypt(string encryptedData);
    }
}