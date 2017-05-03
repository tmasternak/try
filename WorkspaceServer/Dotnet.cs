﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WorkspaceServer
{
    public class Dotnet
    {
        private readonly DirectoryInfo _workingDirectory;

        public Dotnet(DirectoryInfo workingDirectory)
        {
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public void New(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            ExecuteDotnet($"new {templateName}");
        }

        public void Restore()
        {
            ExecuteDotnet("restore");
        }

        public ProcessResult Run()
        {
            return ExecuteDotnet("run");
        }

        public ProcessResult ExecuteDotnet(string args)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = args,
                FileName = DotnetMuxer.Path.FullName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = _workingDirectory.FullName
            });

            process.WaitForExit();

            var stdOut = SplitLines(process.StandardOutput);

            var stdErr = SplitLines(process.StandardError);

            return new ProcessResult
            (
                succeeded: process.ExitCode == 0,
                output: stdOut.Concat(stdErr).ToArray()
            );
        }

        private static IReadOnlyCollection<string> SplitLines(StreamReader reader) =>
            reader.ReadToEnd()
                  .Replace("\r\n", "\n")
                  .Split('\n');
    }
}
