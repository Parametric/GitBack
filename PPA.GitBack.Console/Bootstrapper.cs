using Ninject;

namespace PPA.GitBack.Console
{
    class Bootstrapper
    {
        public static void ConfigureNinjectBindings(IKernel kernel, ProgramOptions programOptions)
        {
            kernel.Bind<IGitApi>().ToMethod(context => new GitApi(programOptions, new GitClientFactory(), new ProcessRunner()));
            kernel.Bind<IGitContext>().To<GitContext>();
        }
    }
}
