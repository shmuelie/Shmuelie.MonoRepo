// <copyright file="MatcherExtensions.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Zio;

namespace Shmuelie.Zio.Glob
{
    /// <summary>
    ///     Extension methods for <see cref="Matcher"/>.
    /// </summary>
    /// <seealso cref="Matcher"/>
    [CLSCompliant(false)]
    public static class MatcherExtensions
    {
        /// <summary>
        ///     Searches the directory specified for all files matching patterns added to this instance of <see cref="Matcher"/>.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <param name="fileSystem">The file system for searching.</param>
        /// <param name="directoryPath">The root directory for the search.</param>
        /// <returns>Absolute file paths of all files matched. Empty enumerable if no files matched given patterns.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="matcher"/> is <see langword="null"/>.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="fileSystem"/> is <see langword="null"/>.</para>
        /// </exception>
        public static IEnumerable<UPath> GetResultsInFullPath(this Matcher matcher, IFileSystem fileSystem, UPath directoryPath)
        {
            if (matcher is null)
            {
                throw new ArgumentNullException(nameof(matcher));
            }

            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            return matcher.Execute(new DirectoryInfo(fileSystem.GetDirectoryEntry(directoryPath))).Files.Select(match => directoryPath / match.Path);
        }

        /// <summary>
        ///     Add a file name pattern that the matcher should use to discover files. Patterns are relative to the root directory given when <see cref="Matcher.Execute(Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase)"/> is called.
        /// </summary>
        /// <param name="matcher">The matcher.</param>
        /// <param name="path">The globbing pattern.</param>
        /// <returns>The matcher instance.</returns>
        /// <remarks><para>Use the forward slash '/' to represent directory separator. Use '*' to represent wildcards in file and directory names. Use '**' to represent arbitrary directory depth. Use '..' to represent a parent directory.</para></remarks>
        /// <exception cref="ArgumentNullException"><paramref name="matcher"/> is <see langword="null"/>.</exception>
        public static Matcher AddInclude(this Matcher matcher, UPath path)
        {
            if (matcher is null)
            {
                throw new ArgumentNullException(nameof(matcher));
            }

            return matcher.AddInclude(path.FullName);
        }
    }
}
