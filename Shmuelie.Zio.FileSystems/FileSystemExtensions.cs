// <copyright file="FileSystemExtensions.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zio;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Extension methods for <see cref="IFileSystem"/>s.
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        ///     Saves the specified file system.
        /// </summary>
        /// <param name="this">The file system to save.</param>
        /// <param name="destSystem">The destination file system.</param>
        /// <param name="destPath">The destination file path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="this"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="destSystem"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="destPath"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <seealso cref="Load(IFileSystem, ZipArchive, CancellationToken)"/>
        [CLSCompliant(false)]
        public static Task Save(this IFileSystem @this, IFileSystem destSystem, UPath destPath, CancellationToken cancellationToken = default) => Save(@this, destSystem, destPath, false, cancellationToken);

        /// <summary>
        ///     Saves the specified file system.
        /// </summary>
        /// <param name="this">The file system to save.</param>
        /// <param name="destSystem">The destination file system.</param>
        /// <param name="destPath">The destination file path.</param>
        /// <param name="overwrite">If set to <see langword="true"/> the destination will overwrite.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="this"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="destSystem"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="destPath"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <seealso cref="Load(IFileSystem, ZipArchive, bool, CancellationToken)"/>
        [CLSCompliant(false)]
        public static Task Save(this IFileSystem @this, IFileSystem destSystem, UPath destPath, bool overwrite, CancellationToken cancellationToken = default)
        {
            if (@this is null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (destSystem is null)
            {
                throw new ArgumentNullException(nameof(destSystem));
            }

            if (destPath.IsNull)
            {
                throw new ArgumentNullException(nameof(destPath));
            }

            return SaveImpl(@this, destSystem, destPath, overwrite, cancellationToken);
        }

        /// <summary>
        ///     Loads the specified zip archive into the file system.
        /// </summary>
        /// <param name="this">The file system to load into.</param>
        /// <param name="zipArchive">The zip archive to load from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous load operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="this"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="zipArchive"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <seealso cref="Save(IFileSystem, IFileSystem, UPath, CancellationToken)"/>
        [CLSCompliant(false)]
        public static Task Load(this IFileSystem @this, ZipArchive zipArchive, CancellationToken cancellationToken = default) => Load(@this, zipArchive, false, cancellationToken);

        /// <summary>
        ///     Loads the specified zip archive into the file system.
        /// </summary>
        /// <param name="this">The file system to load into.</param>
        /// <param name="zipArchive">The zip archive to load from.</param>
        /// <param name="overwrite">If set to <see langword="true"/> overwrite files in the file system.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous load operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="this"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="zipArchive"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <seealso cref="Save(IFileSystem, IFileSystem, UPath, bool, CancellationToken)"/>
        [CLSCompliant(false)]
        public static Task Load(this IFileSystem @this, ZipArchive zipArchive, bool overwrite, CancellationToken cancellationToken = default)
        {
            if (@this is null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (zipArchive is null)
            {
                throw new ArgumentNullException(nameof(zipArchive));
            }

            return LoadImpl(@this, zipArchive, overwrite, cancellationToken);
        }

        private static async Task LoadImpl(IFileSystem @this, ZipArchive zipArchive, bool overwrite, CancellationToken cancellationToken)
        {
            foreach (ZipArchiveEntry archiveEntry in zipArchive.Entries.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                UPath path = new UPath(archiveEntry.FullName).ToAbsolute();
                @this.CreateDirectory(path.GetDirectory());
                using (Stream archiveEntryStream = archiveEntry.Open())
                using (Stream destStream = @this.OpenFile(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await archiveEntryStream.CopyToAsync(destStream, 81920, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static async Task SaveImpl(IFileSystem @this, IFileSystem destSystem, UPath destPath, bool overwrite, CancellationToken cancellationToken)
        {
            using (Stream destStream = destSystem.OpenFile(destPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (ZipArchive archive = new ZipArchive(destStream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                foreach (FileEntry fileEntry in @this.EnumerateFileEntries(UPath.Root, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ZipArchiveEntry archiveEntry = archive.CreateEntry(fileEntry.Path.ToRelative().FullName, CompressionLevel.Optimal);
                    using (Stream fileEntryStream = fileEntry.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    using (Stream archiveEntryStream = archiveEntry.Open())
                    {
                        await fileEntryStream.CopyToAsync(archiveEntryStream, 81920, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
