using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPA.GitBack
{
    public class FakeGitApi : IGitApi
    {
        public FakeGitApi(ProgramOptions programOptions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GitRepository> GetRepositories(string getOwner)
        {
            return new List<GitRepository>
            {
                new GitRepository(this, "github.com/edeng/repo", new DirectoryInfo("directory"), "name")
            };
        }

        public string GetUsername()
        {
            throw new NotImplementedException();
        }

        public string GetOrganization()
        {
            throw new NotImplementedException();
        }

        public void Pull(string url, DirectoryInfo directory, string name)
        {
            Console.WriteLine("git pull: " + "url - " + url + " directory - " + directory);
        }

        public void Clone(string url, DirectoryInfo directory, string name)
        {
            throw new NotImplementedException();
        }

        public DirectoryInfo GetBackupLocation()
        {
            throw new NotImplementedException();
        }
    }
}
