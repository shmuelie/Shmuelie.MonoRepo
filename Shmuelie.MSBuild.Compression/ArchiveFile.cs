// <copyright file="ArchiveFile.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers.GZip;

namespace Shmuelie.MSBuild.Compression
{
    /// <summary>
    /// Creates a GZip from a file.
    /// </summary>
    /// <seealso cref="Task"/>
    public sealed class ArchiveFile : Task
    {
        /// <summary>
        /// Gets or sets a <see cref="ITaskItem"/> containing the full path to the destination file to create.
        /// </summary>
        [Required]
        public ITaskItem? DestinationFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the destination file should be overwritten.
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="ITaskItem"/> containing the full path to the source file to create a gzip archive from.
        /// </summary>
        [Required]
        public ITaskItem? SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the level of compression.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Default;

        /// <inheritdoc />
        public override bool Execute()
        {
            if (DestinationFile is null || SourceFile is null)
            {
                return false;
            }

            FileInfo sourceFile = new FileInfo(SourceFile.ItemSpec);
            if (!sourceFile.Exists)
            {
                Log.LogErrorWithCodeFromResources("ArchiveFile.ErrorFileDoesNotExist", sourceFile.FullName);
                return false;
            }

            FileInfo destinationFile = new FileInfo(DestinationFile.ItemSpec);
            BuildEngine3.Yield();

            try
            {
                if (destinationFile.Exists)
                {
                    if (!Overwrite)
                    {
                        Log.LogErrorWithCodeFromResources("ArchiveFile.ErrorFileExists", destinationFile.FullName);

                        return false;
                    }

                    try
                    {
                        File.Delete(destinationFile.FullName);
                    }
                    catch (Exception e)
                    {
                        Log.LogErrorWithCodeFromResources("ArchiveFile.ErrorFailed", sourceFile.FullName, destinationFile.FullName, e.Message);

                        return false;
                    }

                    try
                    {
                        using (FileStream destinationStream = destinationFile.Create())
                        using (GZipWriter writer = new GZipWriter(destinationStream, new GZipWriterOptions() { CompressionType = SharpCompress.Common.CompressionType.GZip, LeaveStreamOpen = true, CompressionLevel = CompressionLevel }))
                        using (FileStream sourceStream = sourceFile.OpenRead())
                        {
                            writer.Write(sourceFile.Name, sourceStream, sourceFile.LastWriteTime);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.LogErrorWithCodeFromResources("ArchiveFile.ErrorFailed", sourceFile.FullName, destinationFile.FullName, e.Message);
                    }
                }
            }
            finally
            {
                BuildEngine3.Reacquire();
            }

            return !Log.HasLoggedErrors;
        }
    }
}
