namespace GitBack.Credential.Manager {
    public interface IFileStreamer<T>
    {
        T GetObjectOfType(string location);
        void StoreObjectOfType(T recordsToKeeps, string location);
    }
}