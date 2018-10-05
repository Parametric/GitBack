using System.Diagnostics;
using log4net;
using log4net.Core;

namespace GitBack
{
    public class ProcessRunner
    {
        private readonly ILog _logger;

        public ProcessRunner(ILog logger)
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
