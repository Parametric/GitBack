using System;
using log4net.Core;

namespace GitBack.Credential.Manager
{
    public interface ILogger : ILoggerWrapper
    {
        void Debug(object message, Exception exception = null);
        void Info(object message, Exception exception = null);
        void Warn(object message, Exception exception = null);
        void Error(object message, Exception exception = null);

        bool WriteInfoOnErrorString { get; set; }
        bool WriteErrorOnErrorString { get; set; }
        bool WriteWarnOnErrorString { get; set; }
    }
}