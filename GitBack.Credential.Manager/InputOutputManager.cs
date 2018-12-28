using System;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;

namespace GitBack.Credential.Manager {
    public class InputOutputManager : IInputOutputManager
    {
        private readonly bool _removeConsoleAppenders;
        private TextWriter _outputWriter;
        private TextWriter _errorWriter;
        private TextReader _inputReader;
        private bool _disposedValue;

        public bool WriteInfoOnErrorString { get; set; }
        public bool WriteErrorOnErrorString { get; set; }
        public bool WriteWarnOnErrorString { get; set; }

        public InputOutputManager() : this(false) { }

        public InputOutputManager(bool removeConsoleAppenders) : this(false, Console.Out, Console.Error, Console.In) { }

        public InputOutputManager(bool removeConsoleAppenders, TextWriter outputWriter, TextWriter errorWriter, TextReader inputReader)
        {
            _removeConsoleAppenders = removeConsoleAppenders;
            _outputWriter = outputWriter;
            _errorWriter = errorWriter;
            _inputReader = inputReader;
        }

        public IInputOutput GetInputOutput(Type type)
        {
            var log = log4net.LogManager.GetLogger(type);
            var logger = log.Logger;
            if (_removeConsoleAppenders && logger is IAppenderAttachable appenderManager)
            {
                var removeAppenders = from consoleAppender in appenderManager.Appenders.OfType<ConsoleAppender>()
                                      let removedAppender = appenderManager.RemoveAppender(consoleAppender)
                                      select removedAppender.Name;

                var removedAppenders = removeAppenders.ToList();
                if (removedAppenders.Any())
                {
                    log.Warn("Console Appenders can disrupt this application, consider removing them from your log4net configuration.");
                    log.Warn($"Removed the following console appenders: {string.Join(", ", removedAppenders)}");
                }
            }

            return new InputOutput(logger, _outputWriter, _errorWriter, _inputReader)
            {
                WriteErrorOnErrorString = WriteErrorOnErrorString,
                WriteWarnOnErrorString = WriteWarnOnErrorString,
                WriteInfoOnErrorString = WriteInfoOnErrorString,
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue) {
                if (disposing)
                {
                    _inputReader?.Dispose();

                    LogManager.Flush(200);
                    LogManager.Shutdown();

                    _outputWriter?.Flush();
                    _outputWriter?.Dispose();

                    _errorWriter.Flush();
                    _errorWriter?.Dispose();

                }

                _inputReader = null;
                _outputWriter = null;
                _errorWriter = null;

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}