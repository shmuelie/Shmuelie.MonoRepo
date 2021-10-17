// <copyright file="ShellRunner.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Shmuelie.Common
{
    /// <summary>
    ///     Logic for shelling out cross platform.
    /// </summary>
    public static class ShellRunner
    {
        /// <summary>
        ///     Run an executable asynchronously.
        /// </summary>
        /// <param name="executableName">Name of the executable.</param>
        /// <param name="executableRoot">Root location of where to find the executable.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the process.</param>
        /// <returns>The return code of the executable.</returns>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="executableName"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> does not exist.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> contains no usable runtime.</para>
        /// </exception>
        /// <remarks>
        ///     <para>The executable is expected to be found using the <see href="https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks#architecture-specific-folders">NuGet Architecture-Specific logic.</see> under <paramref name="executableRoot"/>.</para>
        /// </remarks>
        public static Task<int> ShellOutAsync(string executableName, string executableRoot, CancellationToken cancellationToken = default) => ShellOutAsync(executableName, executableRoot, string.Empty, null, null, null, cancellationToken);

        /// <summary>
        ///     Run an executable asynchronously.
        /// </summary>
        /// <param name="executableName">Name of the executable.</param>
        /// <param name="executableRoot">Root location of where to find the executable.</param>
        /// <param name="arguments">Arguments to pass to the executable.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the process.</param>
        /// <returns>The return code of the executable.</returns>
        /// <exception cref="ArgumentNullException"><para><paramref name="arguments"/> is <see langword="null"/>.</para></exception>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="executableName"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> does not exist.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> contains no usable runtime.</para>
        /// </exception>
        /// <remarks>
        ///     <para>The executable is expected to be found using the <see href="https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks#architecture-specific-folders">NuGet Architecture-Specific logic.</see> under <paramref name="executableRoot"/>.</para>
        /// </remarks>
        public static Task<int> ShellOutAsync(string executableName, string executableRoot, string arguments, CancellationToken cancellationToken = default) => ShellOutAsync(executableName, executableRoot, arguments, null, null, null, cancellationToken);

        /// <summary>
        ///     Run an executable asynchronously.
        /// </summary>
        /// <param name="executableName">Name of the executable.</param>
        /// <param name="executableRoot">Root location of where to find the executable.</param>
        /// <param name="arguments">Arguments to pass to the executable.</param>
        /// <param name="workingDirectory">Working directory to run the executable in.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the process.</param>
        /// <returns>The return code of the executable.</returns>
        /// <exception cref="ArgumentNullException"><para><paramref name="arguments"/> is <see langword="null"/>.</para></exception>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="executableName"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> does not exist.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> contains no usable runtime.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="workingDirectory"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="workingDirectory"/> does not exist.</para>
        /// </exception>
        /// <remarks>
        ///     <para>The executable is expected to be found using the <see href="https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks#architecture-specific-folders">NuGet Architecture-Specific logic.</see> under <paramref name="executableRoot"/>.</para>
        /// </remarks>
        public static Task<int> ShellOutAsync(string executableName, string executableRoot, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
        {
            if (workingDirectory is null)
            {
                throw new ArgumentException(nameof(workingDirectory));
            }

            return ShellOutAsync(executableName, executableRoot, arguments, workingDirectory, null, null, cancellationToken);
        }

        /// <summary>
        ///     Run an executable asynchronously.
        /// </summary>
        /// <param name="executableName">Name of the executable.</param>
        /// <param name="executableRoot">Root location of where to find the executable.</param>
        /// <param name="arguments">Arguments to pass to the executable.</param>
        /// <param name="workingDirectory">Working directory to run the executable in. If <see langword="null"/>, location of executable will be used.</param>
        /// <param name="standardOutput"><see cref="IProgress{T}"/> for standard out.</param>
        /// <param name="standardError"><see cref="IProgress{T}"/> for standard error.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the process.</param>
        /// <returns>The return code of the executable.</returns>
        /// <exception cref="ArgumentNullException"><para><paramref name="arguments"/> is <see langword="null"/>.</para></exception>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="executableName"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> does not exist.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="executableRoot"/> contains no usable runtime.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="workingDirectory"/> is whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="workingDirectory"/> does not exist.</para>
        /// </exception>
        /// <remarks>
        ///     <para>The executable is expected to be found using the <see href="https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks#architecture-specific-folders">NuGet Architecture-Specific logic.</see> under <paramref name="executableRoot"/>.</para>
        /// </remarks>
        public static Task<int> ShellOutAsync(string executableName, string executableRoot, string arguments, string? workingDirectory, IProgress<string>? standardOutput, IProgress<string>? standardError, CancellationToken cancellationToken = default)
        {
            ValidatParameters(executableName, executableRoot, arguments, workingDirectory);
            string executablePath = NugetSearchFullPathToExecutableAtRoot(executableName, executableRoot);
            DirectoryInfo? directoryInfo = Directory.GetParent(executablePath);
            if (directoryInfo is null)
            {
                throw new BadImageFormatException(); // Should be impossible but...
            }

            return RunProcessAsync(executablePath, arguments, workingDirectory ?? directoryInfo.ToString(), standardOutput, standardError, cancellationToken);
        }

        private static void ValidatParameters(string executableName, string executableRoot, string arguments, string? workingDirectory)
        {
            if (executableName is null)
            {
                throw new ArgumentNullException(nameof(executableName), "Executable name must be specified.");
            }

            if (executableName.All(char.IsWhiteSpace))
            {
                throw new ArgumentException("Executable name must be specified.", nameof(executableName));
            }

            if (executableRoot is null)
            {
                throw new ArgumentNullException(nameof(executableRoot), "Executable root must be specified.");
            }

            if (executableName.All(char.IsWhiteSpace))
            {
                throw new ArgumentException("Executable root must be specified.", nameof(executableRoot));
            }

            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (workingDirectory != null)
            {
                if (workingDirectory.All(char.IsWhiteSpace))
                {
                    throw NullOrWhiteSpaceException(nameof(workingDirectory));
                }

                if (!Directory.Exists(workingDirectory))
                {
                    throw NotExisting(nameof(workingDirectory));
                }
            }
        }

        private static string NugetSearchFullPathToExecutableAtRoot(string executableName, string executableRoot)
        {
            if (string.IsNullOrWhiteSpace(executableName))
            {
                throw NullOrWhiteSpaceException(nameof(executableName));
            }

            if (string.IsNullOrWhiteSpace(executableRoot))
            {
                throw NullOrWhiteSpaceException(nameof(executableRoot));
            }

            if (!Directory.Exists(executableRoot))
            {
                throw NotExisting(nameof(executableRoot));
            }

            string runtimesLocation = Path.Combine(executableRoot, "runtimes");
            RuntimeIdentifier[] runtimeIdentifiers = GetRuntimeIdentifiers(runtimesLocation);
            if (runtimeIdentifiers.Length == 0)
            {
                throw NoUsableRuntime(nameof(executableRoot));
            }

            RuntimeIdentifier rid = RuntimeIdentifier.OSCurrent;
            if (Array.IndexOf(runtimeIdentifiers, rid) == -1)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw NoUsableRuntime(nameof(executableRoot));
                }

                if (!RuntimeIdentifier.TryParse("win-x86", out rid) || Array.IndexOf(runtimeIdentifiers, rid) == -1)
                {
                    throw NoUsableRuntime(nameof(executableRoot));
                }
            }

            string executablePath = Path.Combine(runtimesLocation, rid.ToString(), "native", executableName);
            if (!File.Exists(executablePath))
            {
                throw NoUsableRuntime(nameof(executableRoot));
            }

            return executablePath;
        }

        [SuppressMessage("Usage", "RCS1229:Use async/await when necessary.", Justification = "Not necessary")]
        private static Task<int> RunProcessAsync(string executablePath, string arguments, string workingFolder, IProgress<string>? standardOutput, IProgress<string>? standardError, CancellationToken cancellationToken)
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
            Process? process = null;
            try
            {
                process = new Process()
                {
                    StartInfo = new ProcessStartInfo(executablePath, arguments)
                    {
                        CreateNoWindow = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        WorkingDirectory = workingFolder
                    },
                    EnableRaisingEvents = true
                };
                process.Exited += Exited;
                if (standardOutput is object)
                {
                    process.OutputDataReceived += OutputData;
                }

                if (standardError is object)
                {
                    process.ErrorDataReceived += ErrorData;
                }

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                cancellationToken.Register(OnCancelled);
                return taskCompletionSource.Task;
            }
            catch
            {
                if (process is object)
                {
                    process.Dispose();
                    process.Exited -= Exited;
                    process.ErrorDataReceived -= ErrorData;
                    process.OutputDataReceived -= OutputData;
                }

                throw;
            }

            void OnCancelled()
            {
                process.Exited -= Exited;
                process.ErrorDataReceived -= ErrorData;
                process.OutputDataReceived -= OutputData;
                process.Kill();
                process.Dispose();
                taskCompletionSource.SetCanceled();
            }

            void Exited(object? sender, EventArgs args)
            {
                process.Exited -= Exited;
                process.ErrorDataReceived -= ErrorData;
                process.OutputDataReceived -= OutputData;
                taskCompletionSource.SetResult(process.ExitCode);
                process.Dispose();
            }

            void ErrorData(object? sender, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    standardError?.Report(args.Data + '\n');
                }
            }

            void OutputData(object? sender, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    standardOutput?.Report(args.Data + '\n');
                }
            }
        }

        private static RuntimeIdentifier[] GetRuntimeIdentifiers(string runtimesLocation)
        {
            string[] runtimeFolders = Directory.GetDirectories(runtimesLocation, "*", SearchOption.TopDirectoryOnly);
            List<RuntimeIdentifier> rids = new List<RuntimeIdentifier>(runtimeFolders.Length);
            for (int i = 0; i < runtimeFolders.Length; i++)
            {
                if (RuntimeIdentifier.TryParse(Path.GetFileName(runtimeFolders[i]), out RuntimeIdentifier runtimeIdentifier))
                {
                    rids.Add(runtimeIdentifier);
                }
            }

            return rids.ToArray();
        }

        private static ArgumentException NoUsableRuntime(string parameterName) => new ArgumentException("No usable runtime under executableRoot", parameterName);

        private static ArgumentException NullOrWhiteSpaceException(string parameterName) => new ArgumentException(parameterName + " cannot be null or only whitespace", parameterName);

        private static ArgumentException NotExisting(string parameterName) => new ArgumentException(parameterName + " must be an existing directory.", parameterName);
    }
}
