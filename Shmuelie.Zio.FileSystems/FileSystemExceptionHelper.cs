// <copyright file="FileSystemExceptionHelper.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System.IO;
using Zio;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    /// Helper for creating exceptions in file systems.
    /// </summary>
    internal static class FileSystemExceptionHelper
    {
        /// <summary>
        ///     Create a new <see cref="FileNotFoundException"/>.
        /// </summary>
        /// <param name="path">The path that could not be found.</param>
        /// <returns>A new <see cref="FileNotFoundException"/>.</returns>
        /// <remarks><para>Message is <c>Could not find file `{}`.</c>.</para></remarks>
        public static FileNotFoundException NewFileNotFoundException(UPath path) => new FileNotFoundException($"Could not find file `{path}`.");

        /// <summary>
        ///     Create a new <see cref="DirectoryNotFoundException"/>.
        /// </summary>
        /// <param name="path">The path that could not be found.</param>
        /// <returns>A new <see cref="DirectoryNotFoundException"/>.</returns>
        /// <remarks><para>Message is <c>Could not find a part of the path `{}`.</c>.</para></remarks>
        public static DirectoryNotFoundException NewDirectoryNotFoundException(UPath path) => new DirectoryNotFoundException($"Could not find a part of the path `{path}`.");

        /// <summary>
        ///     Create a new <see cref="IOException"/> for a destination is an existing directory.
        /// </summary>
        /// <param name="path">The path that already exists.</param>
        /// <returns>A new <see cref="IOException"/>.</returns>
        /// <remarks><para>Message is <c>The destination path `{}` is an existing directory.</c>.</para></remarks>
        public static IOException NewDestinationDirectoryExistException(UPath path) => new IOException($"The destination path `{path}` is an existing directory.");

        /// <summary>
        ///     Create a new <see cref="IOException"/> for a destination is an existing file.
        /// </summary>
        /// <param name="path">The path that already exists.</param>
        /// <returns>A new <see cref="IOException"/>.</returns>
        /// <remarks><para>Message is <c>The destination path `{}` is an existing file.</c>.</para></remarks>
        public static IOException NewDestinationFileExistException(UPath path) => new IOException($"The destination path `{path}` is an existing file.");
    }
}
