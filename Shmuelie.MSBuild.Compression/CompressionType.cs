// <copyright file="CompressionType.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

namespace Shmuelie.MSBuild.Compression
{
    /// <summary>
    /// Type of compression to use with <see cref="ArchiveDirectory"/>.
    /// </summary>
    public enum CompressionType
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// GZip compression.
        /// </summary>
        GZip = 1,

        /// <summary>
        /// BZip2 compression.
        /// </summary>
        BZip2 = 2,

        /// <summary>
        /// PPMd compression.
        /// </summary>
        PPMd = 3,

        /// <summary>
        /// Deflate compression.
        /// </summary>
        Deflate = 4,

        /// <summary>
        /// LZMA compression.
        /// </summary>
        LZMA = 6,
    }
}
