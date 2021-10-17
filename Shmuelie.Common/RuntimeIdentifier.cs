// <copyright file="RuntimeIdentifier.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Shmuelie.Common
{
    /// <summary>
    ///     An opaque identifier for a platform.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/core/rid-catalog"/>
    public readonly struct RuntimeIdentifier : IEquatable<RuntimeIdentifier>
    {
        /// <summary>
        ///     Root RID.
        /// </summary>
        /// <value><c>any</c>.</value>
        public static readonly RuntimeIdentifier Any;

        /// <summary>
        ///     Root Windows RID.
        /// </summary>
        /// <value><c>win</c>.</value>
        public static readonly RuntimeIdentifier Win = new RuntimeIdentifier(null, OSPlatform.Windows);

        /// <summary>
        ///     Root macOS RID.
        /// </summary>
        /// <value><c>osx</c>.</value>
        public static readonly RuntimeIdentifier OSX = new RuntimeIdentifier(null, OSPlatform.OSX);

        /// <summary>
        ///     Root Linux RID.
        /// </summary>
        /// <value><c>linux</c>.</value>
        public static readonly RuntimeIdentifier Linux = new RuntimeIdentifier(null, OSPlatform.Linux);

        /// <summary>
        /// The runtime architecture.
        /// </summary>
        private readonly Architecture? _architecture;

        /// <summary>
        /// The runtime platform.
        /// </summary>
        private readonly OSPlatform? _platform;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeIdentifier"/> struct.
        /// </summary>
        /// <param name="architecture">The runtime architecture.</param>
        /// <param name="platform">The runtime platform.</param>
        private RuntimeIdentifier(Architecture? architecture, OSPlatform? platform)
        {
            _architecture = architecture;
            _platform = platform;
        }

        /// <summary>
        ///     Gets the RID for the OS on which the current application is running.
        /// </summary>
        /// <value>
        ///     The RID for the OS on which the current application is running.
        /// </value>
        /// <exception cref="InvalidOperationException">Unknown Platform.</exception>
        public static RuntimeIdentifier OSCurrent
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return new RuntimeIdentifier(RuntimeInformation.OSArchitecture, OSPlatform.Windows);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return new RuntimeIdentifier(RuntimeInformation.OSArchitecture, OSPlatform.Linux);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return new RuntimeIdentifier(RuntimeInformation.OSArchitecture, OSPlatform.OSX);
                }

                throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     Gets the RID of the currently running application.
        /// </summary>
        /// <value>
        ///     The RID of the currently running application.
        /// </value>
        /// <exception cref="InvalidOperationException">Unknown Platform.</exception>
        public static RuntimeIdentifier ProcessCurrent
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return new RuntimeIdentifier(RuntimeInformation.ProcessArchitecture, OSPlatform.Windows);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return new RuntimeIdentifier(RuntimeInformation.ProcessArchitecture, OSPlatform.Linux);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return new RuntimeIdentifier(RuntimeInformation.ProcessArchitecture, OSPlatform.OSX);
                }

                throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     Indicates where one <see cref="RuntimeIdentifier"/> is equal to another <see cref="RuntimeIdentifier"/>.
        /// </summary>
        /// <param name="left">The left <see cref="RuntimeIdentifier"/>.</param>
        /// <param name="right">The right <see cref="RuntimeIdentifier"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="Equals(object)"/>
        /// <seealso cref="Equals(RuntimeIdentifier)"/>
        /// <seealso cref="operator !=(RuntimeIdentifier, RuntimeIdentifier)"/>
        public static bool operator ==(RuntimeIdentifier left, RuntimeIdentifier right) => left.Equals(right);

        /// <summary>
        ///     Indicates where one <see cref="RuntimeIdentifier"/> is not equal to another <see cref="RuntimeIdentifier"/>.
        /// </summary>
        /// <param name="left">The left <see cref="RuntimeIdentifier"/>.</param>
        /// <param name="right">The right <see cref="RuntimeIdentifier"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        /// <seealso cref="Equals(object)"/>
        /// <seealso cref="Equals(RuntimeIdentifier)"/>
        /// <seealso cref="operator ==(RuntimeIdentifier, RuntimeIdentifier)"/>
        public static bool operator !=(RuntimeIdentifier left, RuntimeIdentifier right) => !(left == right);

        /// <summary>
        ///     Try to parse a <see cref="string"/> as a RID.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to parse.</param>
        /// <param name="rid">The resulting <see cref="RuntimeIdentifier"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is a value RID; otherwise <see langword="false"/>.</returns>
        public static bool TryParse(string value, out RuntimeIdentifier rid)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            int dash = value.IndexOf('-');
            string arch;
            string os;
            if (dash == -1)
            {
                arch = "any";
                os = value;
            }
            else if (dash < value.Length - 1)
            {
                os = value.Substring(0, dash);
                arch = value.Substring(dash + 1);
            }
            else
            {
                rid = default;
                return false;
            }

            OSPlatform? platform = null;
            Architecture? architecture = null;
            switch (arch)
            {
                case "any":
                    break;
                case "x86":
                    architecture = Architecture.X86;
                    break;
                case "x64":
                    architecture = Architecture.X64;
                    break;
                case "arm":
                    architecture = Architecture.Arm;
                    break;
                case "arm64":
                    architecture = Architecture.Arm64;
                    break;
                default:
                    rid = default;
                    return false;
            }

            switch (os)
            {
                case "any":
                    break;
                case "win":
                    platform = OSPlatform.Windows;
                    break;
                case "osx":
                    platform = OSPlatform.OSX;
                    break;
                case "linux":
                    platform = OSPlatform.Linux;
                    break;
                default:
                    rid = default;
                    return false;
            }

            if (!platform.HasValue && architecture.HasValue)
            {
                rid = default;
                return false;
            }

            rid = new RuntimeIdentifier(architecture, platform);
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RuntimeIdentifier identifier && Equals(identifier);

        /// <inheritdoc/>
        public bool Equals(RuntimeIdentifier other) => _architecture == other._architecture && _platform == other._platform;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(_architecture, _platform);

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this == Any)
            {
                return "any";
            }

            StringBuilder builder = new StringBuilder(11);
            if (_platform == OSPlatform.Windows)
            {
                builder.Append("win");
            }
            else if (_platform == OSPlatform.OSX)
            {
                builder.Append("osx");
            }
            else if (_platform == OSPlatform.Linux)
            {
                builder.Append("linux");
            }

            if (_architecture.HasValue)
            {
                builder.Append('-');
                switch (_architecture.Value)
                {
                    case Architecture.Arm:
                        builder.Append("arm");
                        break;
                    case Architecture.Arm64:
                        builder.Append("arm64");
                        break;
                    case Architecture.X64:
                        builder.Append("x64");
                        break;
                    case Architecture.X86:
                        builder.Append("x86");
                        break;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Is this instance a generic RID of a specified RID.
        /// </summary>
        /// <param name="other">RID to check.</param>
        /// <returns><see langword="true"/> if <paramref name="other"/> is a more exact RID or the same; otherwise <see langword="false"/>.</returns>
        /// <example>
        ///     <code language="cs">
        /// <![CDATA[
        /// RuntimeIdentifier.TryParse("win-x86", out RuntimeIdentifier win86);
        /// RuntimeIdentifier.Win.IsGenericOf(win86);                 // true
        /// RuntimeIdentifier.OSX.IsGenericOf(win86);                 // false
        /// RuntimeIdentifier.Any.IsGenericOf(win86);                 // true
        /// RuntimeIdentifier.Any.IsGenericOf(RuntimeIdentifier.Win); // true
        /// RuntimeIdentifier.Any.IsGenericOf(RuntimeIdentifier.OSX); // true
        /// ]]>
        ///     </code>
        /// </example>
        public bool IsGenericOf(in RuntimeIdentifier other)
        {
            // Is this above other
            if (this == Any)
            {
                return true;
            }

            if (_platform != other._platform)
            {
                return false;
            }

            return !_architecture.HasValue || _architecture == other._architecture;
        }
    }
}
