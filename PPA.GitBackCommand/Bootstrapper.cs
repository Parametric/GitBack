using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using PPA.GitBack;

namespace PPA.GitBackCommand
{
    class Bootstrapper
    {
        public static void ConfigureNinjectBindings(IKernel kernel, ProgramOptions programOptions)
        {
            var modules = new List<NinjectModule>
            {
                new ProgramModule(programOptions),
            };

            kernel.Load(modules);
        }
    }
}
