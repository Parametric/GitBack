using Octokit;
using Octokit.Internal;

namespace GitBack
{
    public class GitClientFactory
    {
        protected virtual IApiConnection CreateApiConnection(string username, string password)
        {
            var connection = new Connection(new ProductHeaderValue("GitBack"),
                new InMemoryCredentialStore(new Credentials(username, password)));

            var apiConnection = new ApiConnection(connection);
            return apiConnection;
        }

        public virtual IRepositoriesClient CreateGitClient(string username, string password)
        {
            var apiConnection = CreateApiConnection(username, password);

            var repoClient = new RepositoriesClient(apiConnection);
            return repoClient;
        }

        public virtual IUserEmailsClient CreateEmailsClient(string username, string password)
        {
            var apiConnection = CreateApiConnection(username, password);

            var userEmailsClient = new UserEmailsClient(apiConnection);
            return userEmailsClient;
        }
    }
}
