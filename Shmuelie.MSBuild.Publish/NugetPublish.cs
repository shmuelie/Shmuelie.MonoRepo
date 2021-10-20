// <copyright file="NugetPublish.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Commands;
using NuGet.Configuration;

namespace Shmuelie.MSBuild.Publish
{
    /// <summary>
    /// A task to run nuget "push".
    /// </summary>
    /// <seealso cref="Task"/>
    /// <seealso cref="ICancelableTask"/>
    public class NugetPublish : Task, ICancelableTask
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Gets or sets the file paths to the packages to be pushed.
        /// </summary>
        [Required]
        public ITaskItem[] PackagePaths { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Gets or sets the server URL. NuGet identifies a UNC or local folder source and simply copies the file there instead of pushing it using HTTP.
        /// </summary>
        public ITaskItem? Source
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the API key for the server.
        /// </summary>
        public ITaskItem? ApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the symbol server URL.
        /// </summary>
        public ITaskItem? SymbolSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the API key for the symbol server.
        /// </summary>
        public ITaskItem? SymbolApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timeout for pushing to a server in seconds.
        /// </summary>
        /// <remarks><para>Defaults to 300 seconds (5 minutes). Specifying 0 applies the default value.</para></remarks>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets a value indicating whether to disables buffering when pushing to an HTTP(S) server to reduce memory usage.
        /// </summary>
        public bool DisableBuffering
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to not push symbols (even if present).
        /// </summary>
        public bool NoSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to not append "api/v2/package" to the source URL.
        /// </summary>
        public bool NoServiceEndpoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to treats any 409 Conflict response as a warning so that the push can continue.
        /// </summary>
        public bool SkipDuplicate
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            var result = System.Threading.Tasks.Task.Run(ExecuteAsync, cancellationTokenSource.Token);
            result.Wait();
            if (result.IsFaulted)
            {
                Log.LogErrorFromException(result.Exception);
            }

            return result.IsCompleted;
        }

        private System.Threading.Tasks.Task ExecuteAsync()
        {
            ISettings settings = Settings.LoadDefaultSettings(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode));
            return PushRunner.Run(
                settings,
                new PackageSourceProvider(settings),
                PackagePaths.Select(pp => pp.ItemSpec).ToArray(),
                Source?.ItemSpec,
                ApiKey?.ItemSpec,
                SymbolSource?.ItemSpec,
                SymbolApiKey?.ItemSpec,
                TimeoutSeconds,
                DisableBuffering,
                NoSymbols,
                NoServiceEndpoint,
                SkipDuplicate,
                new MSBuildNugetLogger(Log));
        }
    }
}
