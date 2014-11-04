using Ninject.Modules;
using PPA.GitBack;

namespace PPA.GitBackCommand
{
    class ProgramModule : NinjectModule
    {
        private readonly ProgramOptions _programOptions;

        public ProgramModule(ProgramOptions programOptions)
        {
            _programOptions = programOptions;
        }

        public override void Load()
        {
            Kernel.Bind<IGitApi>().ToMethod(context => new GitApi(_programOptions));
        }
    }
}
