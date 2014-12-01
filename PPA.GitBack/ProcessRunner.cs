using System.Diagnostics;

namespace PPA.GitBack
{
    public class ProcessRunner
    {
        public virtual void Run(ProcessStartInfo startInfo)
        {
            var cmdprocess = new Process {StartInfo = startInfo};
            cmdprocess.Start();
            cmdprocess.WaitForExit();
        }
    }
}
