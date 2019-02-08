using System;
using System.Collections.Generic;
using log4net.Core;

namespace GitBack.Credential.Manager
{
    public class InputOutput : IInputOutput
    {
        private readonly Type _declaringType = typeof(InputOutput);
        
        private readonly log4net.Core.ILogger _log;
        private readonly IConsole _console;

        public int WriterWidth => _console.WriterWidth;
        public bool WriteInfoOnErrorString { get; set; }
        public bool WriteErrorOnErrorString { get; set; }
        public bool WriteWarnOnErrorString { get; set; }

        public string Name => _log.Name;

        public InputOutput(log4net.Core.ILogger logger, IConsole console)
        {
            _log = logger;
            _console = console;
        }

        public void WriteOutput(object message = null) => _console.Out.WriteLine(message);

        public IEnumerable<string> ReadInput()
        {
            while (true)
            {
                var line = _console.In.ReadLine();

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
                using (ConsoleColorizer.GetColorizer(level)) { _console.Error.WriteLine($"{level} - {message}"); }
            }
        }

        public bool IsEnabledFor(Level level) => _log.IsEnabledFor(level);

        log4net.Core.ILogger ILoggerWrapper.Logger => _log;

        internal class ConsoleColorizer : IDisposable
        {
            public static ConsoleColorizer GetColorizer(Level level)
            {
                if (level <= Level.Info) { return InfoColorizer; }

                if (level <= Level.Warn) { return WarningColorizer; }

                return ErrorColorizer;
            }

            public static ConsoleColorizer ErrorColorizer => new ConsoleColorizer(ConsoleColor.Red, ConsoleColor.White);
            public static ConsoleColorizer WarningColorizer => new ConsoleColorizer(ConsoleColor.DarkBlue, ConsoleColor.Yellow);
            public static ConsoleColorizer InfoColorizer => new ConsoleColorizer(ConsoleColor.Blue, ConsoleColor.White);


            private ConsoleColor _originalBackgroundColor;
            private ConsoleColor _originalForegroundColor;

            ConsoleColorizer(ConsoleColor backGroundColor, ConsoleColor foregroundColor)
            {
                _originalBackgroundColor = Console.BackgroundColor;
                Console.BackgroundColor = backGroundColor;

                _originalForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
            }


            public void Dispose()
            {
                Console.BackgroundColor = _originalBackgroundColor;
                Console.ForegroundColor = _originalForegroundColor;
            }
        }
    }
}