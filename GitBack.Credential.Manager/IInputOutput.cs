using System.Collections.Generic;

namespace GitBack.Credential.Manager
{
    public interface IInputOutput : ILogger
    {
        int WriterWidth { get; }
        void WriteOutput(object message = null);

        IEnumerable<string> ReadInput();
    }
}