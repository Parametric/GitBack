using ExtendedXmlSerializer.ExtensionModel.Encryption;

namespace GitBack.Credential.Manager {

    public interface IEncryption : ExtendedXmlSerializer.ExtensionModel.Encryption.IEncryption
    {
        string Encrypt(string clearData);

        string Decrypt(string encryptedData);
    }

    public abstract class Encryption : IEncryption
    {
        public string Parse(string data) => Decrypt(data);

        public virtual string Format(string instance) => Encrypt(instance);

        public abstract string Encrypt(string clearData);

        public abstract string Decrypt(string encryptedData);
    }
}