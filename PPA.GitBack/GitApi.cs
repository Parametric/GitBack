using System.Collections.Generic;

namespace PPA.GitBack
{
    public class GitApi : IGitApi
    {
        public GitApi(string userName, string organization = null)
        {
            UserName = userName;
            Organization = organization;
        }

        public string UserName { get; private set; }
        public string Organization { get; private set; }

        public IEnumerable<GitRepository> GetRepositories(string getOwner)
        {
            yield break;
        }

        public string GetUsername()
        {
            return UserName; 
        }

        public string GetOrganization()
        {
            return Organization; 
        }
    }
}