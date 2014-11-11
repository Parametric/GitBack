using Octokit;

namespace PPA.GitBack
{
    public interface IGitClientInitializer
    {
        RepositoriesClient CreateGitClient(string username, string password);
    }
}