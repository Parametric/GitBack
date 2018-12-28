using System;
using System.Collections.Generic;
using System.IO;
using log4net.Core;

namespace GitBack.Credential.Manager {
    public class InputOutput : IInputOutput
    {
        private readonly Type _declaringType = typeof(InputOutput);
        
        private readonly log4net.Core.ILogger _log;
        private readonly TextWriter _outputWriter;
        private readonly TextWriter _errorWriter;
        private readonly TextReader _inputReader;

        public InputOutput(log4net.Core.ILogger logger, TextWriter outputWriter, TextWriter errorWriter, TextReader inputReader)
        {
            _log = logger;

            _outputWriter = outputWriter;
            _errorWriter = errorWriter;
            _inputReader = inputReader;
        }

        public void WriteOutput(object message = null) => _outputWriter.WriteLine(message);

        public IEnumerable<string> ReadInput()
        {
            while (true)
            {
                var line = _inputReader.ReadLine();

                // this loop is not infinite as yield break, will break it.
                if (string.IsNullOrEmpty(line)) { yield break; }

                yield return line;
            }
        }

        public void Debug(object message, Exception exception = null)
            => Log(Level.Debug, message, exception, WriteInfoOnErrorString);

        public void Info(object message, Exception exception = null)
            => Log(Level.Info, message, exception, WriteInfoOnErrorString);

        public void Warn(object message, Exception exception = null)
            => Log(Level.Warn, message, exception, WriteWarnOnErrorString);

        public void Error(object message, Exception exception = null)
            => Log(Level.Error, message, exception, WriteErrorOnErrorString);

        private void Log(Level level, object message, Exception exception, bool writeToOutput)
        {
            _log.Log(_declaringType, level, message, exception);
            if (writeToOutput)
            {
                _errorWriter.WriteLine($"{level} - {message}");
            }
        }

        public bool WriteInfoOnErrorString { get; set; }
        public bool WriteErrorOnErrorString { get; set; }
        public bool WriteWarnOnErrorString { get; set; }
    }
}