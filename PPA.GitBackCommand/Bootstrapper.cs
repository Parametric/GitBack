﻿using Ninject;
using PPA.GitBack;

namespace PPA.GitBackCommand
{
    class Bootstrapper
    {
        public static void ConfigureNinjectBindings(IKernel kernel, ProgramOptions programOptions)
        {
            kernel.Bind<IGitApi>().ToMethod(context => new GitApi(programOptions, new GitClientFactory(), null));
            kernel.Bind<IGitContext>().To<GitContext>();
        }
    }
}
