// <copyright file="CompressedFileSystem.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Zio;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Access a <see cref="ZipArchive"/> as a file system.
    /// </summary>
    /// <seealso cref="FlatFileSystem" />
    [CLSCompliant(false)]
    public class CompressedFileSystem : FlatFileSystem
    {
        /// <summary>
        ///     The archive to access.
        /// </summary>
        private readonly ZipArchive archive;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompressedFileSystem"/> class.
        /// </summary>
        /// <param name="archive">The archive to access.</param>
        /// <exception cref="ArgumentNullException"><paramref name="archive"/> is <see langword="null"/>.</exception>
        public CompressedFileSystem(ZipArchive archive)
        {
            if (archive is null)
            {
                throw new ArgumentNullException(nameof(archive));
            }

            this.archive = archive;
        }

        /// <inheritdoc />
        protected override IEnumerable<UPath> Paths => archive.Entries.Where(e => !string.IsNullOrWhiteSpace(e.Name)).Select(e => UPath.Root / e.FullName);

        /// <inheritdoc />
        protected override void CreateDirectoryImpl(UPath path)
        {
            // NOOP
        }

        /// <inheritdoc />
        protected override void DeleteFileImpl(UPath path) => IfFileExists(path, entry => entry.Delete());

        /// <inheritdoc />
        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            foreach (UPath filePath in this.EnumerateFiles(path, "*", isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                GetEntry(filePath).Delete();
            }
        }

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path) => IfFileExists(path, entry => entry.Length);

        /// <inheritdoc />
        protected override DateTime GetCreationTimeImpl(UPath path) => IfFileExists(path, _ => DefaultFileTime);

        /// <inheritdoc />
        protected override DateTime GetLastWriteTimeImpl(UPath path) => IfFileExists(path, entry => entry.LastWriteTime.UtcDateTime);

        /// <inheritdoc />
        protected override DateTime GetLastAccessTimeImpl(UPath path) => IfFileExists(path, _ => DefaultFileTime);

        /// <inheritdoc />
        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) => IfFileExists(srcPath, srcEntry => CopyEntry(destPath, overwrite, srcEntry));

        /// <inheritdoc />
        protected override void MoveFileImpl(UPath srcPath, UPath destPath) => IfFileExists(srcPath, srcEntry => MoveEntry(destPath, srcEntry));

        /// <inheritdoc />
        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            if (!DirectoryExists(srcPath))
            {
                throw FileSystemExceptionHelper.NewDirectoryNotFoundException(srcPath);
            }

            List<IOException> exceptions = new List<IOException>();
            foreach (UPath srcFilePath in EnumeratePaths(srcPath, "*", SearchOption.AllDirectories, SearchTarget.File))
            {
                UPath destFilePath = destPath / srcFilePath.GetName();
                try
                {
                    MoveEntry(destFilePath, GetEntry(srcFilePath));
                }
                catch (IOException ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }

            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
        }

        /// <inheritdoc />
        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share) => IfFileExists(path, entry => entry.Open());

        /// <inheritdoc />
        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) => IfFileExists(srcPath, srcEntry =>
        {
            if (FileExists(destPath))
            {
                throw FileSystemExceptionHelper.NewDestinationFileExistException(destPath);
            }

            ZipArchiveEntry destEntry = GetEntry(destPath);
            if (!destBackupPath.IsNull)
            {
                CopyEntry(destBackupPath, false, destEntry);
            }

            MoveEntry(destPath, srcEntry);
        });

        /// <inheritdoc />
        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            if (FileExists(path))
            {
                return FileAttributes.Compressed;
            }

            if (DirectoryExists(path))
            {
                return FileAttributes.Compressed | FileAttributes.Directory;
            }

            throw FileSystemExceptionHelper.NewDirectoryNotFoundException(path);
        }

        /// <inheritdoc />
        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }
        }

        /// <inheritdoc />
        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }
        }

        /// <inheritdoc />
        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }
        }

        /// <inheritdoc />
        protected override void SetLastWriteTimeImpl(UPath path, DateTime time) => IfFileExists(path, entry => entry.LastWriteTime = new DateTimeOffset(time));

        /// <inheritdoc />
        protected override bool CanWatchImpl(UPath path) => false;

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath) => innerPath;

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path) => path.FullName;

        /// <inheritdoc />
        protected override IFileSystemWatcher WatchImpl(UPath path) => throw new NotSupportedException();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    archive?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void CopyEntry(UPath destPath, bool overwrite, ZipArchiveEntry srcEntry)
        {
            ZipArchiveEntry destEntry = GetEntry(destPath);
            if (destEntry is null)
            {
                destEntry = archive.CreateEntry(destPath.ToRelative().FullName);
            }
            else if (!overwrite)
            {
                throw FileSystemExceptionHelper.NewDestinationFileExistException(destPath);
            }

            using (Stream src = srcEntry.Open())
            using (Stream dest = destEntry.Open())
            {
                src.CopyTo(dest);
            }
        }

        private void MoveEntry(UPath destPath, ZipArchiveEntry srcEntry)
        {
            CopyEntry(destPath, false, srcEntry);
            srcEntry.Delete();
        }

        private ZipArchiveEntry GetEntry(UPath path) => archive.GetEntry(path.ToRelative().FullName);

        private void IfFileExists(UPath path, Action<ZipArchiveEntry> func)
        {
            if (FileExists(path))
            {
                func(GetEntry(path));
            }
            else
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }
        }

        private T IfFileExists<T>(UPath path, Func<ZipArchiveEntry, T> func)
        {
            if (FileExists(path))
            {
                return func(GetEntry(path));
            }

            throw FileSystemExceptionHelper.NewFileNotFoundException(path);
        }
    }
}
