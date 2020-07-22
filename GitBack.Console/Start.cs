using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;
using log4net;
using log4net.Config;
using Ninject;

namespace GitBack.Console
{
    public static class Start
    {
        private static readonly IKernel Kernel = new StandardKernel();

        public static int Main(string[] args)
        {
            ILog logger = null;
            try
            {
                XmlConfigurator.Configure();
                Bootstrapper.ConfigureLogging(Kernel);
                Bootstrapper.ConfigureParser(Kernel);
                logger = Kernel.Get<ILog>();

                using (var parser = Kernel.Get<Parser>())
                {
                    return parser.ParseArguments<SaveOptions, ConfigOptions, BackupOptions>(args)
                        .MapResult(
                            (SaveOptions opts) => HandleSaveOptions(opts, logger),
                            (ConfigOptions opts) => HandleConfigOptions(opts, logger),
                            (BackupOptions opts) => HandleBackupOptions(opts,logger),
                            errs => 1
                    );
                }
            }
            catch (Exception e)
            {
                logger?.Error("Program failed", e);
                return e.HResult;
            }
        }

        private static int HandleBackupOptions(CommandLineOptions options, ILog logger)
        {
            var programOptions = ConvertCommandLineOptionsToProgramOptions(options, logger);
            Bootstrapper.ConfigureGit(Kernel, programOptions);

            logger.Info("Gitback Backup starting...");

            var program = Kernel.Get<Program>();
            program.Execute();
            logger.Info("Gitback Backup completed...");
            return 0;
        }

        private static int HandleSaveOptions(CommandLineOptions options, ILog logger)
        {
            logger.Info("Gitback Save starting...");
            ConvertCommandLineOptionsToProgramOptions(options, logger);
            Properties.Settings.Default.Save();
            logger.Info("Gitback Save completed...");
            return 0;
        }
        private static int HandleConfigOptions(ConfigOptions options, ILog logger)
        {
            var configProperties = Properties.Settings.Default.ConfigProperties;
            var regex = !string.IsNullOrWhiteSpace(options.UserNameRegex) ? new Regex(options.UserNameRegex) : new Regex(".*");
            logger.Info($"Looking up configs with username matching '{regex}'");
            var usernamesToRemove = new List<string>();
            foreach (ProgramOptions programOption in configProperties)
            {
                if (regex.IsMatch(programOption.Username))
                {
                    logger.Info(programOption);
                    usernamesToRemove.Add(programOption.Username);
                }
            }

            if (options.Remove)
            {
                logger.Info($"Removing user configs");
                if (usernamesToRemove.Count > 1 && !options.Force)
                {
                    logger.Warn($"The Remove Options would remove more than one user config. It would remove: {string.Join(", ",usernamesToRemove)}" + 
                                "If this was intended use the force option.");
                } 
                else if (usernamesToRemove.Count == 0)
                {
                    logger.Warn($"No Users configs found to remove");
                }
                else
                {
                    logger.Info($"Removing user configs: {string.Join(", ", usernamesToRemove)}");
                    foreach (var username in usernamesToRemove)
                    {
                        configProperties.Remove(username);
                    }
                    Properties.Settings.Default.Save();
                }
            }

            logger.Info("Completed Lookup.");
            return 0;
        }

        private static ProgramOptions ConvertCommandLineOptionsToProgramOptions(CommandLineOptions commandLineOptions, ILog logger)
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            var username = commandLineOptions.UserName;
            var configProperties = Properties.Settings.Default.ConfigProperties;
            if (configProperties == null)
            {
                configProperties = new ConfigPropertiesKeyedCollection();
                Properties.Settings.Default.ConfigProperties = configProperties;
            }

            ProgramOptions result = configProperties.GetOrDefault(username);

            var overridenProperties = new List<string>();
            if (!string.IsNullOrWhiteSpace(commandLineOptions.Token))
            {
                result.Token = commandLineOptions.Token; 
                overridenProperties.Add("Token");
            }
            if (!string.IsNullOrWhiteSpace(commandLineOptions.Organization)) 
            {
                result.Organization = commandLineOptions.Organization;
                overridenProperties.Add("Organization");
            }

            if (!string.IsNullOrWhiteSpace(commandLineOptions.BackupLocation))
            {
                result.BackupLocation = new DirectoryInfo(commandLineOptions.BackupLocation); 
                overridenProperties.Add("BackupLocation");
            }

            if (!string.IsNullOrWhiteSpace(commandLineOptions.ProjectFilter))
            {
                result.ProjectFilter = commandLineOptions.ProjectFilter;
                overridenProperties.Add("ProjectFilter");
            }

            if (overridenProperties.Count > 0)
            {
                logger.Info($"The following Properties where loaded from the commandline: {string.Join(", ", overridenProperties)}");
                logger.Info($"Loaded: {result}");
            }

            result.Validate();
            configProperties.AddOrUpdate(result);

            return result;
        }
    }
}