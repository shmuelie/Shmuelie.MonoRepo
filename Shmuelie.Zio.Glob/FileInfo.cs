// <copyright file="FileInfo.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Zio;

namespace Shmuelie.Zio.Glob
{
    /// <summary>
    ///     Represents a file backed by a <see cref="FileEntry"/>.
    /// </summary>
    /// <seealso cref="FileInfoBase"/>
    /// <seealso cref="FileEntry"/>
    [CLSCompliant(false)]
    public sealed class FileInfo : FileInfoBase
    {
        private readonly FileEntry fileEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileInfo"/> class.
        /// </summary>
        /// <param name="fileEntry">Backing <see cref="FileEntry"/>.</param>\
        /// <exception cref="ArgumentNullException"><paramref name="fileEntry"/> is <see langword="null"/>.</exception>
        public FileInfo(FileEntry? fileEntry)
        {
            this.fileEntry = fileEntry ?? throw new ArgumentNullException(nameof(fileEntry));
        }

        /// <inheritdoc/>
        public override string Name => fileEntry.Name;

        /// <inheritdoc/>
        public override string FullName => fileEntry.FullName;

        /// <inheritdoc/>
        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(fileEntry.Parent);
    }
}
