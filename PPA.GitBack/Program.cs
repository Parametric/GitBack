
namespace PPA.GitBack
{
    public class Program
    {
        private readonly GitContext _gitContext;

        public Program(GitContext gitContext)
        {
            _gitContext = gitContext;
        }

        public void Execute()
        {
            _gitContext.BackupAllRepos();
        }
    }
}
