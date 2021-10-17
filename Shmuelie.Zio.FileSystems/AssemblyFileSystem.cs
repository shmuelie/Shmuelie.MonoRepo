// <copyright file="AssemblyFileSystem.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zio;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Present an assembly's resources as a file system.
    /// </summary>
    /// <remarks><para>Exposes Managed Resources only.</para></remarks>
    /// <seealso cref="FlatFileSystem" />
    [CLSCompliant(false)]
    public class AssemblyFileSystem : ReadOnlyFlatFileSystem
    {
        /// <summary>
        ///     The assembly to access resources of.
        /// </summary>
        private readonly Assembly assembly;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyFileSystem"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to access resources of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null" />.</exception>
        public AssemblyFileSystem(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Name = assembly.FullName;
        }

        /// <inheritdoc />
        protected override IEnumerable<UPath> Paths => assembly.GetManifestResourceNames().Select(n => new UPath(n));

        /// <inheritdoc />
        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            if (mode != FileMode.Open || ((access & FileAccess.Write) != 0))
            {
                throw FileSystemIsReadOnly();
            }

            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }

            return assembly.GetManifestResourceStream(path.ToRelative().FullName);
        }

        /// <inheritdoc />
        protected override UPath ConvertPathFromInternalImpl(string innerPath) => innerPath;

        /// <inheritdoc />
        protected override string ConvertPathToInternalImpl(UPath path) => path.FullName;

        /// <inheritdoc />
        protected override long GetFileLengthImpl(UPath path)
        {
            if (!FileExists(path))
            {
                throw FileSystemExceptionHelper.NewFileNotFoundException(path);
            }

            using (Stream strm = OpenFile(path, FileMode.Open, FileAccess.Read))
            {
                return strm.Length;
            }
        }
    }
}
