using System;
using System.Collections.Generic;
using System.IO;

namespace GitBack
{
    public class ProgramOptions
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string Organization { get; set; }
        public DirectoryInfo BackupLocation { get; set; }
        public string ProjectFilter { get; set; }

        public void Validate()
        {
            var missingProperties = new List<string>();
            if (string.IsNullOrWhiteSpace(Username))
            {
                missingProperties.Add(nameof(Username));
            }

            if (string.IsNullOrWhiteSpace(Token))
            {
                missingProperties.Add(nameof(Token));
            }

            if (null == BackupLocation)
            {
                missingProperties.Add(nameof(BackupLocation));
            }

            if (missingProperties.Count > 0)
            {
                throw new ApplicationException(
                    $"The following properties are null or whitespace {string.Join(", ", missingProperties)}");
            }
        }

        public override string ToString() => $"username '{Username}'; {(Token == null ? "No" : "Has")} Token;" +
                                             $" BackupLocation: '{BackupLocation}'; Organization: '{Organization}'; ProjectFilter: '{ProjectFilter}'";
    }
}
