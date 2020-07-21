GitBack
===
Console application used to backup all of your git repos to disk.

Runs nightly as part of a CA job

Installation
===
- Run TeamCity's "Bootstrap Octopus" in your environment to pick up the GitBack-cli role in Octopus

Troubleshooting
===
- Make sure you have git installed
		- This will fix the exception ("Unhandled Exception: System.ComponentModel.Win32Exception: The system cannot find the file specified")

(small change to force update)