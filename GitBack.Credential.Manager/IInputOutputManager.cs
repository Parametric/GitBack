using System;

namespace GitBack.Credential.Manager
{
    public interface IInputOutputManager : ILoggerFactory, IDisposable
    {
        bool WriteInfoOnErrorString { get; set; }
        bool WriteErrorOnErrorString { get; set; }
        bool WriteWarnOnErrorString { get; set; }

        IInputOutput GetInputOutput(Type type);
    }
}