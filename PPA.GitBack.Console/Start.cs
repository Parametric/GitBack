﻿using System.IO;
using CommandLine;
using Ninject;

namespace PPA.GitBack.Console
{
    public static class Start
    {
        public static void Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, options))
            {
                var programOptions = ConvertCommandLineOptionsToProgramOptions(options);
                var kernel = new StandardKernel();

                Bootstrapper.ConfigureNinjectBindings(kernel, programOptions);

                var program = kernel.Get<Program>();
                program.Execute();
            }
        }

        static ProgramOptions ConvertCommandLineOptionsToProgramOptions(CommandLineOptions commandLineOptions)
        {
            return new ProgramOptions
            {
                Username = commandLineOptions.UserName,
                Password = commandLineOptions.Password,
                Organization = commandLineOptions.Organization,
                BackupLocation = new DirectoryInfo(commandLineOptions.BackupLocation),
                PathToGit = commandLineOptions.PathToGit
            };
        }
    }
}