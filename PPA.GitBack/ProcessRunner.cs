using System;
using System.Collections;
using System.Diagnostics;
using PPA.Logging.Contract;

namespace PPA.GitBack
{
    public class ProcessRunner
    {
        private readonly ILogger _logger;

        public ProcessRunner(ILogger logger)
        {
            _logger = logger;
        }

        public virtual void Run(ProcessStartInfo startInfo)
        {
            var process = new Process {StartInfo = startInfo};

            process.Start();            
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                _logger.Info(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                _logger.Error(args.Data);
            };

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
            process.Close();

        }
    }
}
