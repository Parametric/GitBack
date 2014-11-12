using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPA.GitBack
{
    public interface IGitContext
    {
        IEnumerable<IGitRepository> GetRepositories();
        void BackupAllRepos();
    }
}
