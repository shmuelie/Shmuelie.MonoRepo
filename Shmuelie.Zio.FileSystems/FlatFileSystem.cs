// <copyright file="FlatFileSystem.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zio;
using Zio.FileSystems;

namespace Shmuelie.Zio.FileSystems
{
    /// <summary>
    ///     Represents a file system that is backed by list, not a tree, of files.
    /// </summary>
    /// <seealso cref="FileSystem" />
    [CLSCompliant(false)]
    public abstract class FlatFileSystem : FileSystem
    {
        /// <summary>
        ///     Gets the list of paths in this file system.
        /// </summary>
        /// <value>
        ///     The list of paths in this file system.
        /// </value>
        /// <remarks><para>Expected to only return files.</para></remarks>
        protected abstract IEnumerable<UPath> Paths
        {
            get;
        }

        /// <summary>
        ///     Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><see langword="true"/> if path contains the name of an existing file; otherwise, <see langword="false"/>.</returns>
        /// <remarks><para>This method also returns <see langword="false"/> if path is <see langword="null"/>, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns <see langword="false"/> regardless of the existence of path.</para></remarks>
        /// <exception cref="ArgumentException"><paramref name="path"/> is relative.</exception>
        protected sealed override bool FileExistsImpl(UPath path)
        {
            path.AssertAbsolute();
            return EnumeratePaths(path.GetDirectory(), path.GetName(), SearchOption.TopDirectoryOnly, SearchTarget.File).Any();
        }

        /// <summary>
        ///     Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><see langword="true"/> if the given path refers to an existing directory on disk, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is relative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        protected sealed override bool DirectoryExistsImpl(UPath path)
        {
            path.AssertAbsolute();
            return path == UPath.Root || EnumeratePaths(path.GetDirectory(), path.GetName(), SearchOption.TopDirectoryOnly, SearchTarget.Directory).Any();
        }

        /// <summary>
        ///     Returns an enumerable collection of file names and/or directory names that match a search pattern in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="path">The path to the directory to search.</param>
        /// <param name="searchPattern">The search string to match against file-system entries in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
        /// <param name="searchTarget">The search target either <see cref="SearchTarget.Both" /> or only <see cref="SearchTarget.Directory" /> or <see cref="SearchTarget.File" />.</param>
        /// <returns>An enumerable collection of file-system paths in the directory specified by path and that match the specified search pattern, option and target.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is relative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="searchPattern"/> is <see langword="null" />.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="path"/> does not exist.</exception>
        protected sealed override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            SearchPattern search = SearchPattern.Parse(ref path, ref searchPattern);
            return EnumerateItems(path, searchOption, (ref FileSystemItem item) =>
            {
                return ((item.IsDirectory && (searchTarget == SearchTarget.Both || searchTarget == SearchTarget.Directory)) ||
                       (!item.IsDirectory && (searchTarget == SearchTarget.Both || searchTarget == SearchTarget.File))) && search.Match(item.AbsolutePath);
            }).Select(item => item.AbsolutePath);
        }

        /// <inheritdoc/>
        protected override IEnumerable<FileSystemItem> EnumerateItemsImpl(UPath path, SearchOption searchOption, SearchPredicate? searchPredicate)
        {
            var directories = new SortedSet<UPath>(UPath.DefaultComparerIgnoreCase);

            foreach (UPath resourceName in Paths)
            {
                var fileItem = new FileSystemItem(this, resourceName, false);
                if (resourceName.IsInDirectory(path, searchOption == SearchOption.AllDirectories) && (searchPredicate?.Invoke(ref fileItem) ?? true))
                {
                    yield return fileItem;
                }

                UPath resourceDirectory = resourceName.GetDirectory();
                if (directories.Add(resourceDirectory))
                {
                    var directotyItem = new FileSystemItem(this, resourceDirectory, true);
                    if (resourceDirectory.IsInDirectory(path, searchOption == SearchOption.AllDirectories) && (searchPredicate?.Invoke(ref directotyItem) ?? false))
                    {
                        yield return directotyItem;
                    }
                }
            }
        }
    }
}
