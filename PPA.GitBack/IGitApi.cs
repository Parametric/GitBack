using System.Collections.Generic;

namespace PPA.GitBack
{
    public interface IGitApi
    {
        IEnumerable<GitRepository> GetRepositories(string getOwner);

        string GetUsername();

        string GetOrganization(); 
    }
}