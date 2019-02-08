using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace GitBack.Credential.Manager
{
    public class InputOutputManager : IInputOutputManager
    {
        private static readonly string TypeName = typeof(InputOutputManager).ToString();

        private readonly Dictionary<string, InputOutput> _loggers = new Dictionary<string, InputOutput>();
        private readonly bool _removeConsoleAppenders;
        private IConsole _console;
        private IInputOutput _logger;
        private bool _disposedValue;
        
        private bool _hasChanged;
        private bool _writeInfo;
        private bool _writeError;
        private bool _writeWarn;

        public bool WriteInfoOnErrorString
        {
            get => _writeInfo;
            set
            {
                if (value != _writeInfo) { _hasChanged = true; }
                _writeInfo = value;
            }
        }

        public bool WriteErrorOnErrorString
        {
            get => _writeError;
            set
            {
                if (value != _writeError) { _hasChanged = true; }

                _writeError = value;
            }
        }

        public bool WriteWarnOnErrorString
        {
            get => _writeWarn;
            set
            {
                if (value != _writeWarn) { _hasChanged = true; }

                _writeWarn = value;
            }
        }

        public InputOutputManager(bool removeConsoleAppenders, IConsole console)
        {
            _removeConsoleAppenders = removeConsoleAppenders;
            _console = console;

            InitializeLogger();
        }

        private void InitializeLogger()
        {
            if (!_loggers.ContainsKey(TypeName))
            {
                _logger = GetNewInputOutput(TypeName, false);
                RemoveAppenders(TypeName, _logger);
                _loggers.Add(TypeName, (InputOutput) _logger);
            }

            if (_hasChanged)
            {
                foreach (var logger in _loggers.Values)
                {
                    logger.WriteErrorOnErrorString = WriteErrorOnErrorString;
                    logger.WriteWarnOnErrorString = WriteWarnOnErrorString;
                    logger.WriteInfoOnErrorString = WriteInfoOnErrorString;
                }
            }
        }

        public IInputOutput GetInputOutput(string typeFullName)
        {
            InitializeLogger();
            if (!_loggers.ContainsKey(typeFullName))
            {
                var newInputOutput = GetNewInputOutput(typeFullName);
                _loggers.Add(typeFullName, newInputOutput);
            }

            var inputOutput = _loggers[typeFullName];

            return inputOutput;
        }

        private InputOutput GetNewInputOutput(string typeFullName, bool removeAppenders = true)
        {
            var log = LogManager.GetLogger(typeFullName);
            var logger = log.Logger;

            var inputOutput = new InputOutput(logger, _console)
            {
                WriteErrorOnErrorString = WriteErrorOnErrorString,
                WriteWarnOnErrorString = WriteWarnOnErrorString,
                WriteInfoOnErrorString = WriteInfoOnErrorString
            };

            if (removeAppenders) { RemoveAppenders(typeFullName, inputOutput); }

            return inputOutput;
        }

        private void RemoveAppenders(string typeFullName, ILoggerWrapper loggerWrapper)
        {
            var appenderLogger = loggerWrapper.Logger;
            var removedAppenderNames = new List<string>();
            while (appenderLogger != null)
            {
                if (_removeConsoleAppenders && appenderLogger is IAppenderAttachable appenderManager)
                {
                    var appendersToRemove = appenderManager.Appenders.OfType<ConsoleAppender>().ToList();

                    foreach (var removeAppender in appendersToRemove)
                    {
                        appenderManager.RemoveAppender(removeAppender);
                        removedAppenderNames.Add(removeAppender.Name);
                        removeAppender.Close();
                    }
                }

                if (appenderLogger is Logger appenderLoggerAsLogger) { appenderLogger = appenderLoggerAsLogger.Parent; }
                else { appenderLogger = null; }
            }

            if (removedAppenderNames.Any())
            {
                _logger.Warn("Console Appenders can disrupt this application, consider removing them from your log4net configuration.");
                _logger.Warn($"For log of {typeFullName}, Removed the following console appenders: {string.Join(", ", removedAppenderNames)}");
            }
        }

        public ILogger GetLogger(Type type) => GetLogger(type.FullName);
        public IInputOutput GetInputOutput(Type type) => GetInputOutput(type.FullName);
        public ILogger GetLogger(string typeFullname) => GetInputOutput(typeFullname);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    LogManager.Flush(200);
                    LogManager.Shutdown();
                    _console.Dispose();
                }

                _console = null;

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