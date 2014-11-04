using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPA.GitBack
{
    public class Program
    {
        private readonly ProgramOptions _programOptions;

        public Program(ProgramOptions programOptions)
        {
            _programOptions = programOptions;
        }

        public void Execute()
        {
            var gitApi = new FakeGitApi(_programOptions.RootGitUrl());
            var gitContext = new GitContext(gitApi);

        }
    }
}
