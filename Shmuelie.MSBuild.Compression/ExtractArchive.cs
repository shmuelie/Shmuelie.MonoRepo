// <copyright file="ExtractArchive.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Shmuelie.MSBuild.Compression
{
    /// <summary>
    /// Represents a task that can extract an archive.
    /// </summary>
    /// <example>
    ///     <para>The following example unzips an archive and overwrites any read-only files.</para>
    ///     <code language="xml">
    ///     <![CDATA[<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///
    ///     <Target Name = "UnzipArchive" BeforeTargets="Build">
    ///         <Extract
    ///             SourceFiles = "MyArchive.zip"
    ///             DestinationFolder="$(OutputPath)\unzipped"
    ///             OverwriteReadOnlyFiles="true"
    ///         />
    ///     </Target>
    ///
    /// </Project>]]>
    ///     </code>
    /// </example>
    /// <seealso cref="Task"/>
    /// <seealso cref="ICancelableTask"/>
    public sealed class ExtractArchive : Task, ICancelableTask
    {
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        private const int DefaultCopyBufferSize = 81920;

        /// <summary>
        /// Stores a <see cref="CancellationTokenSource"/> used for cancellation.
        /// </summary>
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        /// <summary>
        /// Gets or sets a <see cref="ITaskItem"/> with a destination folder path to unzip the files to.
        /// </summary>
        [Required]
        public ITaskItem? DestinationFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether read-only files should be overwritten.
        /// </summary>
        public bool OverwriteReadOnlyFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether files should be skipped if the destination is unchanged.
        /// </summary>
        public bool SkipUnchangedFiles { get; set; } = true;

        /// <summary>
        /// Gets or sets an array of <see cref="ITaskItem"/> objects containing the paths to .zip archive files to unzip.
        /// </summary>
        [Required]
        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Nullable notation.")]
        public ITaskItem[]? SourceFiles { get; set; }

        /// <inheritdoc />
        public void Cancel() => cancellationToken.Cancel();

        /// <inheritdoc />
        public override bool Execute()
        {
            if (DestinationFolder is null)
            {
                return false;
            }

            DirectoryInfo destinationDirectory;
            try
            {
                destinationDirectory = Directory.CreateDirectory(DestinationFolder.ItemSpec);
            }
            catch (Exception e)
            {
                Log.LogErrorWithCodeFromResources("Extract.ErrorCouldNotCreateDestinationDirectory", DestinationFolder.ItemSpec, e.Message);

                return false;
            }

            BuildEngine3.Yield();

            try
            {
                foreach (string sourceFile in SourceFiles.TakeWhile(_ => !cancellationToken.IsCancellationRequested).Select(taskItem => taskItem.ItemSpec))
                {
                    if (!File.Exists(sourceFile))
                    {
                        Log.LogErrorWithCodeFromResources("Extract.ErrorFileDoesNotExist", sourceFile);
                        continue;
                    }

                    try
                    {
                        using (FileStream stream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0x1000, useAsync: false))
                        {
                            using (IReader reader = ReaderFactory.Open(stream))
                            {
                                try
                                {
                                    Extract(reader, destinationDirectory);
                                }
                                catch (Exception e)
                                {
                                    // Unhandled exception in Extract() is a bug!
                                    Log.LogErrorFromException(e, showStackTrace: true);
                                    return false;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        // Should only be thrown if the archive could not be opened (Access denied, corrupt file, etc)
                        Log.LogErrorWithCodeFromResources("Extract.ErrorCouldNotOpenFile", sourceFile, e.Message);
                    }
                }
            }
            finally
            {
                BuildEngine3.Reacquire();
            }

            return !cancellationToken.IsCancellationRequested && !Log.HasLoggedErrors;
        }

        /// <summary>
        /// Extracts all files to the specified directory.
        /// </summary>
        /// <param name="reader">The <see cref="IReader"/> containing the files to extract.</param>
        /// <param name="destinationDirectory">The <see cref="DirectoryInfo"/> to extract files to.</param>
        private void Extract(IReader reader, DirectoryInfo destinationDirectory)
        {
            while (reader.MoveToNextEntry() && !cancellationToken.IsCancellationRequested)
            {
                IEntry entry = reader.Entry;

                FileInfo destinationPath = new FileInfo(Path.Combine(destinationDirectory.FullName, entry.Key));

                // Zip archives can have directory entries listed explicitly.
                // If this entry is a directory we should create it and move to the next entry.
                if (Path.GetFileName(destinationPath.FullName).Length == 0)
                {
                    // The entry is a directory
                    Directory.CreateDirectory(destinationPath.FullName);
                    continue;
                }

                if (!destinationPath.FullName.StartsWith(destinationDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    // ExtractToDirectory() throws an IOException for this but since we're extracting one file at a time
                    // for logging and cancellation, we need to check for it ourselves.
                    Log.LogErrorFromResources("Extract.ErrorExtractingResultsInFilesOutsideDestination", destinationPath.FullName, destinationDirectory.FullName);
                    continue;
                }

                if (ShouldSkipEntry(entry, destinationPath))
                {
                    Log.LogMessageFromResources(MessageImportance.Low, "Extract.DidNotExtractBecauseOfFileMatch", entry.Key, destinationPath.FullName, nameof(SkipUnchangedFiles), "true");
                    continue;
                }

                try
                {
                    destinationPath.Directory?.Create();
                }
                catch (Exception e)
                {
                    Log.LogErrorWithCodeFromResources("Extract.ErrorCouldNotCreateDestinationDirectory", destinationPath.DirectoryName, e.Message);
                    continue;
                }

                if (OverwriteReadOnlyFiles && destinationPath.Exists && destinationPath.IsReadOnly)
                {
                    try
                    {
                        destinationPath.IsReadOnly = false;
                    }
                    catch (Exception e)
                    {
                        Log.LogErrorWithCodeFromResources("Extract.ErrorCouldNotMakeFileWriteable", entry.Key, destinationPath.FullName, e.Message);
                        continue;
                    }
                }

                try
                {
                    Log.LogMessageFromResources(MessageImportance.Normal, "Extract.FileComment", entry.Key, destinationPath.FullName);

                    using (Stream destination = File.Open(destinationPath.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (Stream stream = reader.OpenEntryStream())
                    {
                        stream.CopyToAsync(destination, DefaultCopyBufferSize, cancellationToken.Token)
                            .ConfigureAwait(continueOnCapturedContext: false)
                            .GetAwaiter()
                            .GetResult();
                    }

                    if (entry.LastModifiedTime.HasValue)
                    {
                        destinationPath.LastWriteTimeUtc = entry.LastModifiedTime.Value;
                    }
                }
                catch (IOException e)
                {
                    Log.LogErrorWithCodeFromResources("Extract.ErrorCouldNotExtractFile", entry.Key, destinationPath.FullName, e.Message);
                }
            }
        }

        /// <summary>
        /// Determines whether or not a file should be skipped when extracting.
        /// </summary>
        /// <param name="entry">The <see cref="IEntry"/> object containing information about the file in the archive.</param>
        /// <param name="fileInfo">A <see cref="FileInfo"/> object containing information about the destination file.</param>
        /// <returns><see langword="true"/> if the file should be skipped, otherwise <see langword="false"/>.</returns>
        private bool ShouldSkipEntry(IEntry entry, FileInfo fileInfo)
        {
            return SkipUnchangedFiles
                   && fileInfo.Exists
                   && entry.LastModifiedTime == fileInfo.LastWriteTimeUtc
                   && entry.Size == fileInfo.Length;
        }
    }
}
