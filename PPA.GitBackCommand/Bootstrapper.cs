using Ninject;
using PPA.GitBack;

namespace PPA.GitBackCommand
{
    class Bootstrapper
    {
        public static void ConfigureNinjectBindings(IKernel kernel, ProgramOptions programOptions)
        {
            kernel.Bind<IGitApi>().ToMethod(context => new GitApi(programOptions));
            kernel.Bind<IGitContext>().ToMethod(context => new GitContext(new GitApi(programOptions)));
        }
    }
}
