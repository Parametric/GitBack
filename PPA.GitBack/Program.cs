
namespace PPA.GitBack
{
    public class Program
    {
        private readonly IGitContext _gitContext;

        public Program(IGitContext gitContext)
        {
            _gitContext = gitContext;
        }

        public void Execute()
        {
            _gitContext.BackupAllRepos();
        }
    }
}
