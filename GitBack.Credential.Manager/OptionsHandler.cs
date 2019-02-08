using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace GitBack.Credential.Manager
{
    public class OptionsHandler : IDisposable
    {
        private static readonly ShowLoggerOutPut[] DefaultLoggerOptions = { ShowLoggerOutPut.None, ShowLoggerOutPut.Error };

        private readonly IInputOutput _inputOutput;
        private bool _disposedValue;
        private IDisposable _ndcContext;
        private readonly ICredentialRecordsManager _credentialRecordsManager;
        private readonly IInputOutputManager _inputOutputManager;

        public OptionsHandler(ICredentialRecordsManager credentialRecordsManager, IInputOutputManager inputOutputManager)
        {
            SetupNdc();

            _inputOutputManager = inputOutputManager;
            InitializeInputOutputManager();
            _inputOutput = _inputOutputManager.GetInputOutput(typeof(OptionsHandler));
            _credentialRecordsManager = credentialRecordsManager;
        }

        private void SetupNdc()
        {
            var processId = Process.GetCurrentProcess().Id;
            var domainId = Thread.GetDomainID();
            var threadId = Thread.CurrentThread.ManagedThreadId;

            var ndcContext = $"Process:{processId}.d{domainId}.t{threadId}";

            _ndcContext = ThreadContext.Stacks["NDC"].Push(ndcContext);
        }

        private void InitializeInputOutputManager(IEnumerable<ShowLoggerOutPut> loggerOptions = null)
        {
            var manager = _inputOutputManager;

            var myLoggerOptions = loggerOptions == null
                ? DefaultLoggerOptions
                : loggerOptions is ICollection<ShowLoggerOutPut> loggerOptionsCollection
                    ? loggerOptionsCollection
                    : loggerOptions.ToArray();

            if (myLoggerOptions.IsEmpty())
            {
                myLoggerOptions = DefaultLoggerOptions;
            }

            foreach (var loggerOption in myLoggerOptions)
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
        }

        private static bool ShouldListenOnStdIn(CredentialHelperOptions options, ICredentialRecord record)
            => (options.Listen == YesNo.Yes || (options.Listen == YesNo.Default && record.IsEmpty() && options.Operation != Operation.List));

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

            if (!string.IsNullOrEmpty(arg.ReportLocation))
            {
                _credentialRecordsManager.RecordsLocation = new FileInfo(arg.ReportLocation);
            }

            var record = _credentialRecordsManager.GetCredentialRecordFromOptions(arg);

            if (ShouldListenOnStdIn(arg, record)) { FillRecordFromStdIn(record); }

            switch (arg.Operation)
            {
                case Operation.List:
                    var recordList = _credentialRecordsManager.ListRecords(record);
                    foreach (var outRecord in recordList)
                    {
                        _inputOutput.WriteOutput(outRecord);
                    }
                    break;
                case Operation.Erase:
                    _credentialRecordsManager.EraseRecords(record);
                    break;
                case Operation.Get:
                    var outGetRecord = _credentialRecordsManager.GetRecord(record);
                    _inputOutput.WriteOutput(outGetRecord);
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


        public int HandleErrors(ParserResult<CredentialHelperOptions> parserResult)
        {
            if (!(parserResult is NotParsed<CredentialHelperOptions> notParsed))
            {
                throw new ArgumentException($"Unknown {nameof(parserResult)}: {parserResult}", nameof(parserResult));
            }

            var helpText = HelpText.AutoBuild(notParsed, _inputOutput.WriterWidth);
            helpText.AddEnumValuesToHelpText = true;
            helpText.AddOptions(parserResult);

            _inputOutput.WriteOutput(helpText);

            var errors = notParsed.Errors.ToList();
            var versionOrHelpRequested =
                errors.Any(e => e.Tag == ErrorType.VersionRequestedError ||
                                e.Tag == ErrorType.HelpRequestedError ||
                                e.Tag == ErrorType.HelpVerbRequestedError);
            return versionOrHelpRequested ? 0 : 1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _credentialRecordsManager?.Dispose();
                    _inputOutputManager?.Dispose();
                    _ndcContext?.Dispose();
                }

                _ndcContext = null;
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