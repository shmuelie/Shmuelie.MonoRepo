// <copyright file="DirectoryInfo.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Zio;

namespace Shmuelie.Zio.Glob
{
    /// <summary>
    ///     Represents a directory backed by a <see cref="DirectoryEntry"/>.
    /// </summary>
    /// <seealso cref="DirectoryInfoBase"/>
    /// <seealso cref="DirectoryEntry"/>
    [CLSCompliant(false)]
    public sealed class DirectoryInfo : DirectoryInfoBase
    {
        private readonly DirectoryEntry directoryEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryInfo"/> class.
        /// </summary>
        /// <param name="directoryEntry">Backing <see cref="DirectoryEntry"/>.</param>\
        /// <exception cref="ArgumentNullException"><paramref name="directoryEntry"/> is <see langword="null"/>.</exception>
        public DirectoryInfo(DirectoryEntry? directoryEntry)
        {
            this.directoryEntry = directoryEntry ?? throw new ArgumentNullException(nameof(directoryEntry));
        }

        /// <inheritdoc/>
        public override string Name => directoryEntry.Name;

        /// <inheritdoc/>
        public override string FullName => directoryEntry.FullName;

        /// <inheritdoc/>
        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(directoryEntry.Parent);

        /// <inheritdoc/>
        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos() => directoryEntry.EnumerateEntries().Select(ConvertToZio);

        /// <inheritdoc/>
        public override DirectoryInfoBase GetDirectory(string path) => new DirectoryInfo(new DirectoryEntry(directoryEntry.FileSystem, directoryEntry.Path / path));

        /// <inheritdoc/>
        public override FileInfoBase GetFile(string path) => new FileInfo(new FileEntry(directoryEntry.FileSystem, directoryEntry.Path / path));

        private static FileSystemInfoBase ConvertToZio(FileSystemEntry fileSystemEntry)
        {
            if (fileSystemEntry is DirectoryEntry directoryEntry)
            {
                return new DirectoryInfo(directoryEntry);
            }

            if (fileSystemEntry is FileEntry fileEntry)
            {
                return new FileInfo(fileEntry);
            }

            throw new NotSupportedException();
        }
    }
}
