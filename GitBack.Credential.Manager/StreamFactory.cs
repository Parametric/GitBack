using System.IO;

namespace GitBack.Credential.Manager
{
    public class StreamFactory : IStreamFactory
    {
        public Stream GetStream(FileInfo filePath)
        {
            var parent = filePath.Directory;
            if (parent != null && !parent.Exists)
            {
                parent.Create();
            }

            return new FileStream(filePath.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
    }
}