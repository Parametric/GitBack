using System.Collections;
using System.Collections.Generic;

namespace PPA.GitBack
{
    public class GitContext
    {
        private readonly string _username;
        private readonly GitApi _gitApi;
        private readonly string _organization;

        public GitContext(GitApi gitApi)
        {

            _gitApi = gitApi;
            _username = gitApi.UserName;
            _organization = gitApi.Organization;
        }

        public string GetOwner()
        {
            return string.IsNullOrWhiteSpace(_organization) ? _username : _organization;
        }

        public IEnumerable<GitRepository> GetRepositories()
        {
            return null;
        }
    }
}