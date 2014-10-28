using System.Collections;
using System.Collections.Generic;

namespace PPA.GitBack
{
    public class GitContext
    {
        private readonly IGitApi _gitApi;

        public GitContext(IGitApi gitApi)
        {

            _gitApi = gitApi;

        }

        public string GetOwner()
        {
            var organization = _gitApi.GetOrganization();
            var username = _gitApi.GetUsername();

            return string.IsNullOrWhiteSpace(organization)
                ? username
                : organization;
        }

        public IEnumerable<GitRepository> GetRepositories()
        {
            return _gitApi.GetRepositories(GetOwner());
        }
    }
}