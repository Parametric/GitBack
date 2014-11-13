using System.Diagnostics;

namespace PPA.GitBack
{
    public interface IProcessRunner
    {
        void Run(ProcessStartInfo startInfo);
    }
}