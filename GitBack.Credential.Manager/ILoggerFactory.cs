using System;

namespace GitBack.Credential.Manager
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(Type type);
    }
}