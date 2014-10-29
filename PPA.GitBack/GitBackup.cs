using System.IO;

namespace PPA.GitBack
{
    public class GitBackup : IGitBackup
    {
        private readonly IGitRepository _gitRepository;

        public GitBackup(IGitRepository gitRepository)
        {
            _gitRepository = gitRepository;
        }

        public void Backup(string directory)
        {
            if (_gitRepository.ExistsInDirectory(directory))
            {
                _gitRepository.Pull();
            }
            else
            {
                _gitRepository.Clone();
            }
        }
    }
}
