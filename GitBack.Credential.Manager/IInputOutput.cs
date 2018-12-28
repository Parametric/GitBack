using System.Collections.Generic;

namespace GitBack.Credential.Manager {
    public interface IInputOutput : ILogger
    {
        void WriteOutput(object message = null);

        IEnumerable<string> ReadInput();
    }
}