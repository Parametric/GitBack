﻿using Octokit;
using Octokit.Internal;

namespace GitBack
{
    public class GitClientFactory
    {
        public virtual IRepositoriesClient CreateGitClient(string username, string password)
        {

            var connection = new Connection(new ProductHeaderValue("GitBack"),
               new InMemoryCredentialStore(new Credentials(username, password)));

            var apiConnection = new ApiConnection(connection);

            var repoClient = new RepositoriesClient(apiConnection);
            return repoClient; 
        }
    }
}
