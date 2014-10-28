using System.Collections;
using System.Collections.Generic;

namespace PPA.GitBack
{
    public class GitApi
    {
        public GitApi(string userName, string organization = null)
        {
            
        }

        public IEnumerable<GitRepository> GetRepositories(string getOwner)
        {
            yield break;
        }

        public string UserName { get; private set; }
        public string Organization { get; private set; }
    }
}