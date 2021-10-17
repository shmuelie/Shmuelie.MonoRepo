// <copyright file="ArchiveType.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

namespace Shmuelie.MSBuild.Compression
{
    /// <summary>
    /// Type of archive to create with <see cref="ArchiveDirectory"/>.
    /// </summary>
    public enum ArchiveType
    {
        /// <summary>
        /// Zip Archive.
        /// </summary>
        Zip = 1,

        /// <summary>
        /// Tar Archive.
        /// </summary>
        Tar = 2,
    }
}
