using System;
using System.IO;

namespace GitBack.Credential.Manager
{
    public interface IConsole : IDisposable
    {
        TextWriter Out { get; }
        TextWriter Error { get; }

        TextReader In { get; }

        int WriterWidth { get; }
    }
}