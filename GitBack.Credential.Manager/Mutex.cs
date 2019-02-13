using System;

namespace GitBack.Credential.Manager {
    internal class Mutex : IMutex
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

    public interface IMutexFactory
    {
        IMutex GetMutex(string name);
    }

    public class MutexFactory : IMutexFactory
    {
        private readonly TimeSpan _mutexTimeout;

        public MutexFactory(int timeOutSeconds = 45) : this(TimeSpan.FromSeconds(timeOutSeconds)) { }

        public MutexFactory(TimeSpan timeout)
        {
            if (timeout.Ticks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "timeout cannot be negative");
            }

            _mutexTimeout = timeout;
        }

        public IMutex GetMutex(string name)
        {
            if (name == null) { throw new ArgumentNullException(nameof(name)); }

            var safeName = name.Replace(@"\", ":");
            return new Mutex(safeName, _mutexTimeout);
        }
    }
}