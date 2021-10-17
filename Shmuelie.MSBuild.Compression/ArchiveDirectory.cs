// <copyright file="ArchiveDirectory.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SharpCompress.Writers;

namespace Shmuelie.MSBuild.Compression
{
    /// <summary>
    /// Creates an archive from the contents of a directory.
    /// </summary>
    /// <example>
    ///     <para>The following example (if used as an imported .targets file) creates a .zip archive from the output directory after building a project. The <c>$(OutputPath)</c> property would normally be defined in an MSBuild project file, so a project file that imports the following file would produce a zip archive <c>output.zip</c>:</para>
    ///     <code language="xml">
    ///     <![CDATA[
    ///     <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///
    ///     <Target Name = "ZipOutputPath" AfterTargets="Build">
    ///        <ZipDirectory
    ///            SourceDirectory = "$(OutputPath)"
    ///            DestinationFile="$(MSBuildProjectDirectory)\output.zip" />
    ///     </Target>
    ///
    ///     </Project>
    ///     ]]>
    ///     </code>
    /// </example>
    /// <seealso cref="Task"/>
    public sealed class ArchiveDirectory : Task
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
        /// Gets or sets a <see cref="ITaskItem"/> containing the full path to the source directory to create an archive from.
        /// </summary>
        [Required]
        public ITaskItem? SourceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the type of archive to create. If not set, <see cref="ArchiveType.Zip"/> is created.
        /// </summary>
        public ArchiveType ArchiveFormat { get; set; } = ArchiveType.Zip;

        /// <summary>
        /// Gets or sets the type of compression to use. If not set, <see cref="CompressionType.Deflate"/> is used.
        /// </summary>
        public CompressionType CompressionFormat { get; set; } = CompressionType.Deflate;

        /// <inheritdoc />
        public override bool Execute()
        {
            if (SourceDirectory is null || DestinationFile is null)
            {
                return false;
            }

            if (!VarifyArchiveCompression())
            {
                return false;
            }

            DirectoryInfo sourceDirectory = new DirectoryInfo(SourceDirectory.ItemSpec);
            if (!sourceDirectory.Exists)
            {
                Log.LogErrorWithCodeFromResources("ArchiveDirectory.ErrorDirectoryDoesNotExist", sourceDirectory.FullName);
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
                        Log.LogErrorWithCodeFromResources("ArchiveDirectory.ErrorFileExists", destinationFile.FullName);

                        return false;
                    }

                    try
                    {
                        File.Delete(destinationFile.FullName);
                    }
                    catch (Exception e)
                    {
                        Log.LogErrorWithCodeFromResources("ArchiveDirectory.ErrorFailed", sourceDirectory.FullName, destinationFile.FullName, e.Message);

                        return false;
                    }
                }

                try
                {
                    Log.LogMessageFromResources(MessageImportance.High, "ArchiveDirectory.Comment", sourceDirectory.FullName, destinationFile.FullName);

                    using (FileStream archiveStream = destinationFile.OpenWrite())
                    using (IWriter writer = WriterFactory.Open(archiveStream, (SharpCompress.Common.ArchiveType)ArchiveFormat, new WriterOptions((SharpCompress.Common.CompressionType)CompressionFormat) { LeaveStreamOpen = true }))
                    {
                        writer.WriteAll(sourceDirectory.FullName, "*", SearchOption.AllDirectories);
                    }
                }
                catch (Exception e)
                {
                    Log.LogErrorWithCodeFromResources("ArchiveDirectory.ErrorFailed", sourceDirectory.FullName, destinationFile.FullName, e.Message);
                }
            }
            finally
            {
                BuildEngine3.Reacquire();
            }

            return !Log.HasLoggedErrors;
        }

        private bool VarifyArchiveCompression()
        {
            switch (CompressionFormat)
            {
                case CompressionType.None:
                case CompressionType.BZip2:
                    return true;
            }

            if (ArchiveFormat == ArchiveType.Tar && CompressionFormat == CompressionType.GZip)
            {
                return true;
            }

            if (ArchiveFormat == ArchiveType.Zip)
            {
                switch (CompressionFormat)
                {
                    case CompressionType.Deflate:
                    case CompressionType.LZMA:
                    case CompressionType.PPMd:
                        return true;
                }
            }

            Log.LogErrorWithCodeFromResources("ArchiveDirectory.InvalidArchiveCompression", ArchiveFormat, CompressionFormat);
            return false;
        }
    }
}
