// <copyright file="ResourceFileSystem.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shmuelie.Native;
using Shmuelie.Zio.FileSystems;
using Windows.Win32;
using Windows.Win32.System.LibraryLoader;
using Zio;

namespace Shmuelie.Zio.Windows
{
    /// <summary>
    ///     Present a library's resources as a file system.
    /// </summary>
    /// <remarks><para>Exposes Unmanaged Resources only.</para></remarks>
    /// <seealso cref="ReadOnlyFlatFileSystem"/>
    [CLSCompliant(false)]
    public sealed class ResourceFileSystem : ReadOnlyFlatFileSystem
    {
        private static readonly Dictionary<string, ushort> LangaugeNameToID = Win32Helpers.GetLanguages().ToDictionary(p => p.name, p => p.id);
        private readonly FreeLibrarySafeHandle library;
        private readonly UPath[] paths;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceFileSystem"/> class.
        /// </summary>
        /// <param name="path">Path to the library to wrap.</param>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="path"/> is <see langword="null"/> or whitespace.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="path"/> does not exist.</para>
        /// </exception>
        public ResourceFileSystem(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new ArgumentException($"'{nameof(path)}' must be an existing file.", nameof(path));
            }

            library = PInvoke.LoadLibrary(path);
            paths = ResourceHelpers.GetResources(library).Select(tri => UPath.Root / Win32Helpers.GetLanguageName(tri.language) / (tri.name + '.' + tri.type)).ToArray();
        }

        /// <inheritdoc />
        protected override IEnumerable<UPath> Paths => paths;

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            return UPath.Root / innerPath;
        }

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path)
        {
            return path.ToRelative().FullName;
        }

        /// <inheritdoc />
        protected override unsafe Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            (string? type, string name, ushort language) = GetResourceTrifecta(path);
            HRSRC resourceInfo = PInvoke.FindResourceEx(library, type, name, language);
            uint size = PInvoke.SizeofResource(library, resourceInfo);
            IntPtr startingPointer = PInvoke.LoadResource(library, resourceInfo);
            return new UnmanagedMemoryStream((byte*)startingPointer.ToPointer(), size, size, FileAccess.Read);
        }

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path)
        {
            (string? type, string name, ushort language) = GetResourceTrifecta(path);
            HRSRC resourceInfo = PInvoke.FindResourceEx(library, type, name, language);
            return PInvoke.SizeofResource(library, resourceInfo);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    library?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private static (string? type, string name, ushort language) GetResourceTrifecta(UPath path)
        {
            string? type = path.GetExtensionWithDot()?.Substring(1);
            string langaugeName = path.GetFirstDirectory(out UPath name);
            return (type, name.FullName, LangaugeNameToID[langaugeName]);
        }
    }
}
