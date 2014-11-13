using System.Diagnostics;

namespace PPA.GitBack
{
    public class ProcessRunner : IProcessRunner
    {
        public void Run(ProcessStartInfo startInfo)
        {
            var cmdprocess = new Process {StartInfo = startInfo};
            cmdprocess.Start();
        }
    }
}
