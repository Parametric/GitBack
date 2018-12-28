using System;

namespace GitBack.Credential.Manager {
    public class Mutex : IMutex
    {
        private readonly TimeSpan _waitOneTimeout;
        private readonly System.Threading.Mutex _mutex;

        public Mutex(string name, TimeSpan waitOneTimeout)
        {
            _waitOneTimeout = waitOneTimeout;
            _mutex = new System.Threading.Mutex(false, name);
        }

        public void Dispose() => _mutex.Dispose();

        public bool WaitOne() => WaitOne(_waitOneTimeout);

        public bool WaitOne(TimeSpan timeSpan) => _mutex.WaitOne(timeSpan);

        public void ReleaseMutex() => _mutex.ReleaseMutex();
    }
}