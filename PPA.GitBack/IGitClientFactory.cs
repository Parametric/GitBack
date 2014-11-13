using Octokit;

namespace PPA.GitBack
{
    public interface IGitClientFactory
    {
        IRepositoriesClient CreateGitClient(string username, string password);
    }
}