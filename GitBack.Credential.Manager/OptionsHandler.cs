using System;

namespace GitBack.Credential.Manager {
    public class OptionsHandler : IDisposable
    {
        private readonly IInputOutput _inputOutput;
        private bool _disposedValue = false;
        private readonly ICredentialRecordsManager _credentialRecordsManager;
        private readonly IInputOutputManager _inputOutputManager;

        public OptionsHandler(ICredentialRecordsManager credentialRecordsManager, IInputOutputManager inputOutputManager)
        {
            _inputOutput = inputOutputManager.GetInputOutput(typeof(OptionsHandler));
            _credentialRecordsManager = credentialRecordsManager;
            _inputOutputManager = inputOutputManager;
        }

        private void InitializeInputOutputManager(ShowLoggerOutPut[] loggerOptions)
        {
            var manager = _inputOutputManager;

            if (loggerOptions == null || loggerOptions.Length == 0)
            {
                loggerOptions = new[] { ShowLoggerOutPut.None, ShowLoggerOutPut.Error };
            }

            foreach (var loggerOption in loggerOptions)
            {
                switch (loggerOption)
                {
                    case ShowLoggerOutPut.None:
                        manager.WriteErrorOnErrorString = manager.WriteWarnOnErrorString = manager.WriteInfoOnErrorString = false;
                        break;
                    case ShowLoggerOutPut.All:
                        manager.WriteErrorOnErrorString = manager.WriteWarnOnErrorString = manager.WriteInfoOnErrorString = true;
                        break;
                    case ShowLoggerOutPut.Error:
                        manager.WriteErrorOnErrorString = true;
                        break;
                    case ShowLoggerOutPut.Warn:
                        manager.WriteWarnOnErrorString = true;
                        break;
                    case ShowLoggerOutPut.Info:
                        manager.WriteInfoOnErrorString = true;
                        break;
                    default:
                        _inputOutput.WriteWarnOnErrorString = manager.WriteWarnOnErrorString;
                        _inputOutput.Warn($"Unknown Show Logger Option {loggerOption}");
                        break;
                }
            }

            _inputOutput.WriteErrorOnErrorString = manager.WriteErrorOnErrorString;
            _inputOutput.WriteWarnOnErrorString = manager.WriteWarnOnErrorString;
            _inputOutput.WriteInfoOnErrorString = manager.WriteInfoOnErrorString;
        }

        private static bool ShouldListenOnStdIn(CredentialHelperOptions options, ICredentialRecord record)
            => (options.Listen == YesNo.Yes || (options.Listen == YesNo.Default && record.IsEmpty()));

        private void FillRecordFromStdIn(ICredentialRecord record)
        {
            _inputOutput.Info("Listening for user input. Listening terminated by a Blank Line");
            foreach (var input in _inputOutput.ReadInput())
            {
                var indexOfFirstOfEquals = input.IndexOf('=');
                if (indexOfFirstOfEquals < 0)
                {
                    _inputOutput.Warn("Input from standard in must contain an '='. See: https://git-scm.com/docs/git-credential#IOFMT");
                }
                else if (indexOfFirstOfEquals == 0)
                {
                    _inputOutput.Warn("Input from standard in must contain an attribute before the '='. See: https://git-scm.com/docs/git-credential#IOFMT");
                }
                else
                {
                    var attribute = input.Substring(0, indexOfFirstOfEquals);

                    var valueIndex = indexOfFirstOfEquals + 1;
                    var valueLength = input.Length - valueIndex;
                    var value = input.Substring(valueIndex, valueLength);

                    if (string.IsNullOrEmpty(value))
                    {
                        _inputOutput.Warn($"Found nothing after the '=', value is empty, this will remove the {attribute} attribute, if it was already added");
                    }

                    record.AddOrUpdatePropertyValue(attribute, value);
                }
            }
        }

        public int HandleOptions(CredentialHelperOptions arg)
        {
            InitializeInputOutputManager(arg.LoggerOutput);

            if (arg == null) { throw new ArgumentNullException(nameof(arg));}

            var record = arg.ConvertToCredentialRecord();

            if (ShouldListenOnStdIn(arg, record)) { FillRecordFromStdIn(record); }

            switch (arg.Operation)
            {
                case Operation.List:
                    _credentialRecordsManager.ListRecords(record);
                    break;
                case Operation.Erase:
                    _credentialRecordsManager.EraseRecords(record);
                    break;
                case Operation.Get:
                    _credentialRecordsManager.GetRecord(record);
                    break;
                case Operation.Store:
                    _credentialRecordsManager.StoreRecord(record);
                    break;
                default:
                    _inputOutput.Warn($"Unknown Show Logger Operation option {arg.Operation}");
                    break;
            }

            return 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _credentialRecordsManager.Dispose();
                    _inputOutputManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

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