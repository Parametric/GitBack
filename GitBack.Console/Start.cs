using log4net.Config;
using Ninject;

namespace GitBack.Console
{
    public static class Start
    {
        public static int Main(string[] args)
        {
            XmlConfigurator.Configure();
            Bootstrapper.ConfigureNinjectBindings();
            var argumentParser = Bootstrapper.Kernel.Get<IArgumentParser>();
            var result = argumentParser.ParseArguments(args);
            return result;
        }
    }
}