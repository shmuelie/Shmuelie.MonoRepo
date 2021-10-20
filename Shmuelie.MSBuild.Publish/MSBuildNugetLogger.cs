// <copyright file="MSBuildNugetLogger.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using Task = System.Threading.Tasks.Task;

namespace Shmuelie.MSBuild.Publish
{
    internal class MSBuildNugetLogger : LoggerBase
    {
        private readonly TaskLoggingHelper taskLoggingHelper;

        public MSBuildNugetLogger(TaskLoggingHelper taskLoggingHelper)
        {
            this.taskLoggingHelper = taskLoggingHelper;
        }

        public override void Log(ILogMessage message)
        {
            switch (message.Level)
            {
                case LogLevel.Error:
                    taskLoggingHelper.LogError(null, $"NU{(int)message.Code}", null, message.ProjectPath, 0, 0, 0, 0, message.FormatWithCode());
                    break;
                case LogLevel.Warning:
                    taskLoggingHelper.LogWarning(message.WarningLevel.ToString(), $"NU{(int)message.Code}", null, message.ProjectPath, 0, 0, 0, 0, message.FormatWithCode());
                    break;
                case LogLevel.Minimal:
                    taskLoggingHelper.LogMessage(null, $"NU{(int)message.Code}", null, message.ProjectPath, 0, 0, 0, 0, MessageImportance.High, message.FormatWithCode());
                    break;
                case LogLevel.Information:
                    taskLoggingHelper.LogMessage(null, $"NU{(int)message.Code}", null, message.ProjectPath, 0, 0, 0, 0, MessageImportance.Normal, message.FormatWithCode());
                    break;
                case LogLevel.Verbose:
                    taskLoggingHelper.LogMessage(null, $"NU{(int)message.Code}", null, message.ProjectPath, 0, 0, 0, 0, MessageImportance.Low, message.FormatWithCode());
                    break;
            }
        }

        public override Task LogAsync(ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }
    }
}
