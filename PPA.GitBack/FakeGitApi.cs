using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPA.GitBack
{
    class FakeGitApi : IGitApi
    {
        public FakeGitApi(string rootGitUrl)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GitRepository> GetRepositories(string getOwner)
        {
            throw new NotImplementedException();
        }

        public string GetUsername()
        {
            throw new NotImplementedException();
        }

        public string GetOrganization()
        {
            throw new NotImplementedException();
        }

        public void Pull(string url, DirectoryInfo directory)
        {
            Console.WriteLine("git pull url directory");
        }

        public void Clone(string url, DirectoryInfo directory)
        {
            throw new NotImplementedException();
        }
    }
}
