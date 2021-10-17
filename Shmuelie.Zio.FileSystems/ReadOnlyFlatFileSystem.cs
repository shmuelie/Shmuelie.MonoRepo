// <copyright file="ReadOnlyFlatFileSystem.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.IO;
using Zio;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Base class for <see cref="FlatFileSystem"/>s that are readonly.
    /// </summary>
    /// <seealso cref="FlatFileSystem" />
    [CLSCompliant(false)]
    public abstract class ReadOnlyFlatFileSystem : FlatFileSystem
    {
        /// <summary>
        ///     Create a new <see cref="IOException"/> for the file system being readonly.
        /// </summary>
        /// <returns>A new <see cref="IOException"/> for the file system being readonly.</returns>
        protected static IOException FileSystemIsReadOnly() => new IOException("This file system is read-only");

        /// <inheritdoc />
        protected override void CreateDirectoryImpl(UPath path) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void MoveFileImpl(UPath srcPath, UPath destPath) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void DeleteFileImpl(UPath path) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetAttributesImpl(UPath path, FileAttributes attributes) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetCreationTimeImpl(UPath path, DateTime time) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetLastAccessTimeImpl(UPath path, DateTime time) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override void SetLastWriteTimeImpl(UPath path, DateTime time) => throw FileSystemIsReadOnly();

        /// <inheritdoc />
        protected override bool CanWatchImpl(UPath path) => false;

        /// <inheritdoc />
        protected override IFileSystemWatcher WatchImpl(UPath path) => throw new NotSupportedException();

        /// <inheritdoc />
        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            if (FileExists(path))
            {
                return FileAttributes.ReadOnly;
            }

            if (DirectoryExists(path))
            {
                return FileAttributes.Directory | FileAttributes.ReadOnly;
            }

            throw FileSystemExceptionHelper.NewDirectoryNotFoundException(path);
        }

        /// <inheritdoc />
        protected override DateTime GetCreationTimeImpl(UPath path) => DefaultFileTime;

        /// <inheritdoc />
        protected override DateTime GetLastAccessTimeImpl(UPath path) => DefaultFileTime;

        /// <inheritdoc />
        protected override DateTime GetLastWriteTimeImpl(UPath path) => DefaultFileTime;
    }
}
