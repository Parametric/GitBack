using System.IO;

namespace GitBack.Credential.Manager
{
    public interface IStreamFactory
    {
        Stream GetStream(FileInfo filePath);
    }
}