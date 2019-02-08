using System;
using System.Data;
using System.IO;

namespace GitBack.Credential.Manager {
    public class GitBackConsole : IConsole
    {
        public static int DefaultWriterWidth = 100;
        private readonly Func<int> _writerWidth;
        private bool _disposedValue;

        public GitBackConsole(TextWriter outputWriter, TextWriter errorWriter, TextReader inputReader) :
            this(outputWriter, errorWriter, inputReader, () => DefaultWriterWidth) { }

        public GitBackConsole(TextWriter outputWriter, TextWriter errorWriter, TextReader inputReader, Func<int> writerWidth)
        {
            Out = outputWriter;
            Error = errorWriter;
            In = inputReader;
            _writerWidth = writerWidth;
        }

        public TextWriter Out { get; }
        public TextWriter Error { get; }
        public TextReader In { get; }

        public ILogger Logger { get; set; }

        public int WriterWidth
        {
            get
            {
                try { return _writerWidth(); }
                catch (Exception e)
                {
                    Logger?.Error($"_writerWidth func failed with: {e.Message}", e);
                    return DefaultWriterWidth;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue && disposing)
            {
                In?.Dispose();
                Out?.Flush();
                Out?.Dispose();
                Error?.Flush();
                Error?.Dispose();

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    internal class GitBackStandardConsole : GitBackConsole
    {
        private static int GetWindowWidth() => Console.WindowWidth;

        public GitBackStandardConsole() : base(Console.Out, Console.Error, Console.In, GetWindowWidth) { }
    }
}