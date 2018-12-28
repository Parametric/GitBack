using System;

namespace GitBack.Credential.Manager {
    public interface IMutex : IDisposable
    {
        bool WaitOne();
        void ReleaseMutex();
    }
}