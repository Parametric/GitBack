using System;
using CommandLine;
using Ninject;

namespace GitBack.Credential.Manager
{
    public class Program
    {
        public static int Main(string[] args)
        {
            ILogger logger = null;
            try
            {
                Bootstrapper.ConfigureBindings();
                using (var kernel = Bootstrapper.Kernel)
                {
                    logger = kernel.Get<ILogger>();

                    using (var optionsHandler = kernel.Get<OptionsHandler>())
                    using (var parser = kernel.Get<Parser>())
                    {
                        var parserResult = parser.ParseArguments<CredentialHelperOptions>(args);

                        return parserResult is Parsed<CredentialHelperOptions> parsedResult
                            ? optionsHandler.HandleOptions(parsedResult.Value)
                            : optionsHandler.HandleErrors(parserResult);
                    }
                }
            }
            catch (Exception e)
            {
                logger?.Error($"Caught Exception {e}", e);
                return e.HResult == 0 ? -1 : e.HResult;
            }
        }
    }

    public enum Operation
    {
        Get,
        Store,
        Erase,
        List
    }

    public enum YesNo
    {
        Default,
        Yes,
        No,
    }

    public enum ShowLoggerOutPut
    {
        None,
        Info,
        Warn,
        Error,
        All,
    }
}

