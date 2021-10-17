// <copyright file="Uuid.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Shmuelie.Common.Converters;

namespace Shmuelie.Common
{
    /// <summary>
    ///     An Universally Unique IDentifier.
    /// </summary>
    /// <remarks>
    ///     <para>Represents a RFC-4122 compliant Universally Unique IDentifier (UUID). An UUID is a 128-bit integer (16 bytes) that can be used across all computers and networks wherever a unique identifier is required. Such an identifier has a very low probability of being duplicated.</para>
    ///     <para>A <see cref="Guid"/> is a <see cref="Uuid"/>, version 4.</para>
    /// </remarks>
    /// <seealso cref="IEquatable{T}" />
    /// <seealso cref="IComparable{T}"/>
    /// <seealso cref="IComparable"/>
    /// <seealso cref="Guid"/>
    /// <seealso href="https://tools.ietf.org/html/rfc4122">RFC-4122</seealso>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(UuidConverter))]
    [Serializable]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct Uuid : IEquatable<Uuid>, IComparable<Uuid>, IComparable
    {
        /// <summary>
        ///     A UUID with all bits set to zero.
        /// </summary>
        /// <value>00000000-0000-0000-0000-000000000000.</value>
        /// <remarks><para>Same as <see cref="Uuid.Uuid()"/>.</para></remarks>
        public static readonly Uuid Nil;

        /// <summary>
        ///     Name string is a fully-qualified domain name.
        /// </summary>
        /// <value>6ba7b810-9dad-11d1-80b4-00c04fd430c8.</value>
        public static readonly Uuid DNS = new Uuid(0x6ba7b810, 0x9dad, 0x11d1, 0x80, 0xb4, 0x00c0, 0x4fd430c8);

        /// <summary>
        ///     Name string is a URL.
        /// </summary>
        /// <value>6ba7b811-9dad-11d1-80b4-00c04fd430c8.</value>
        public static readonly Uuid Url = new Uuid(0x6ba7b811, 0x9dad, 0x11d1, 0x80, 0xb4, 0x00c0, 0x4fd430c8);

        /// <summary>
        ///     Name string is an ISO OID.
        /// </summary>
        /// <value>6ba7b812-9dad-11d1-80b4-00c04fd430c8.</value>
        public static readonly Uuid Oid = new Uuid(0x6ba7b812, 0x9dad, 0x11d1, 0x80, 0xb4, 0x00c0, 0x4fd430c8);

        /// <summary>
        ///     Name string is an X.500 DN (in DER or a text output format).
        /// </summary>
        /// <value>6ba7b814-9dad-11d1-80b4-00c04fd430c8.</value>
        public static readonly Uuid X500 = new Uuid(0x6ba7b814, 0x9dad, 0x11d1, 0x80, 0xb4, 0x00c0, 0x4fd430c8);

        /// <summary>
        ///     The byte offset for <see cref="_time_low"/>.
        /// </summary>
        /// <value>0.</value>
        private const int TimeLowOffset = 0;

        /// <summary>
        ///     The byte offset for <see cref="_time_mid"/>.
        /// </summary>
        /// <value>4.</value>
        private const int TimeMidOffset = 4;

        /// <summary>
        ///     The byte offset for <see cref="_time_hi_and_version"/>.
        /// </summary>
        /// <value>6.</value>
        private const int TimeHiAndVersionOffset = 6;

        /// <summary>
        ///     The byte offset for <see cref="_clock_seq_hi_and_reserved"/>.
        /// </summary>
        /// <value>8.</value>
        private const int ClockSeqHiAndReservedOffset = 8;

        /// <summary>
        ///     The byte offset for <see cref="_clock_seq_low"/>.
        /// </summary>
        /// <value>9.</value>
        private const int ClockSeqLowOffset = 9;

        /// <summary>
        ///     The byte offset for <see cref="_node_hi"/>.
        /// </summary>
        /// <value>10.</value>
        private const int NodeHiOffset = 10;

        /// <summary>
        ///     The byte offset for <see cref="_node_low"/>.
        /// </summary>
        /// <value>12.</value>
        private const int NodeLowOffset = 12;

        /// <summary>
        ///     The bit offset for the version number.
        /// </summary>
        /// <value>12.</value>
        private const int VersionOffset = 12;

        /// <summary>
        ///     The bit mask for accessing the high field of the timestamp in <see cref="_time_hi_and_version"/>.
        /// </summary>
        /// <value>0b0000_0000_0000_0000_0000_1111_1111_1111.</value>
        private const int TimeHighMask = 0b1111_1111_1111;

        /// <summary>
        ///     The bit mask for accessing the high field of the clock sequence in <see cref="_clock_seq_hi_and_reserved"/>.
        /// </summary>
        private const int ClockSeqHighMask = 0b0011_1111;

        /// <summary>
        ///     Length of a <see cref="Uuid"/> in bytes.
        /// </summary>
        private const int ByteLength = 16;

        /// <summary>
        ///     The low field of the timestamp.
        /// </summary>
        private readonly uint _time_low;

        /// <summary>
        ///     The middle field of the timestamp.
        /// </summary>
        private readonly ushort _time_mid;

        /// <summary>
        ///     The high field of the timestamp multiplexed with the version number.
        /// </summary>
        private readonly ushort _time_hi_and_version;

        /// <summary>
        ///     The high field of the clock sequence multiplexed with the variant.
        /// </summary>
        private readonly byte _clock_seq_hi_and_reserved;

        /// <summary>
        ///     The low field of the clock sequence.
        /// </summary>
        private readonly byte _clock_seq_low;

        /// <summary>
        ///     The first two bytes of the spatially unique node identifier.
        /// </summary>
        private readonly ushort _node_hi;

        /// <summary>
        ///     The last four bytes of the spatially unique node identifier.
        /// </summary>
        private readonly uint _node_low;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Uuid"/> struct.
        /// </summary>
        /// <param name="time_low">The low field of the timestamp.</param>
        /// <param name="time_mid">The middle field of the timestamp.</param>
        /// <param name="time_hi_and_version">The high field of the timestamp multiplexed with the version number.</param>
        /// <param name="clock_seq_hi_and_reserved">The high field of the clock sequence multiplexed with the variant.</param>
        /// <param name="clock_seq_low">The low field of the clock sequence.</param>
        /// <param name="node_hi">The first two bytes of the spatially unique node identifier.</param>
        /// <param name="node_low">The last four bytes of the spatially unique node identifier.</param>
        private Uuid(uint time_low, ushort time_mid, ushort time_hi_and_version, byte clock_seq_hi_and_reserved, byte clock_seq_low, ushort node_hi, uint node_low)
        {
            _time_low = time_low;
            _time_mid = time_mid;
            _time_hi_and_version = time_hi_and_version;
            _clock_seq_hi_and_reserved = clock_seq_hi_and_reserved;
            _clock_seq_low = clock_seq_low;
            _node_hi = node_hi;
            _node_low = node_low;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Uuid"/> struct.
        /// </summary>
        /// <param name="time_low">The low field of the timestamp.</param>
        /// <param name="time_mid">The middle field of the timestamp.</param>
        /// <param name="time_hi_and_version">The high field of the timestamp multiplexed with the version number.</param>
        /// <param name="clock_seq_hi_and_reserved">The high field of the clock sequence multiplexed with the variant.</param>
        /// <param name="clock_seq_low">The low field of the clock sequence.</param>
        /// <param name="node">The spatially unique node identifier.</param>
        private unsafe Uuid(uint time_low, ushort time_mid, ushort time_hi_and_version, byte clock_seq_hi_and_reserved, byte clock_seq_low, byte[] node)
        {
            _time_low = time_low;
            _time_mid = time_mid;
            _time_hi_and_version = time_hi_and_version;
            _clock_seq_hi_and_reserved = clock_seq_hi_and_reserved;
            _clock_seq_low = clock_seq_low;
            fixed (byte* b = node)
            {
                _node_hi = *(ushort*)(b + NodeHiOffset);
                _node_low = *(uint*)(b + NodeLowOffset);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Uuid"/> struct.
        /// </summary>
        /// <param name="bytes">A 16-element byte array containing values with which to initialize the UUID.</param>
        private unsafe Uuid(byte[] bytes)
            : this(bytes, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Uuid"/> struct.
        /// </summary>
        /// <param name="bytes">A byte array containing values with which to initialize the UUID.</param>
        /// <param name="offset">The offset within <paramref name="bytes"/> to read from.</param>
        private unsafe Uuid(byte[] bytes, int offset)
        {
            fixed (byte* b = bytes)
            {
                _time_low = *(uint*)(b + TimeLowOffset + offset);
                _time_mid = *(ushort*)(b + TimeMidOffset + offset);
                _time_hi_and_version = *(ushort*)(b + TimeHiAndVersionOffset + offset);
                _clock_seq_hi_and_reserved = *(b + ClockSeqHiAndReservedOffset + offset);
                _clock_seq_low = *(b + ClockSeqLowOffset + offset);
                _node_hi = *(ushort*)(b + NodeHiOffset + offset);
                _node_low = *(uint*)(b + NodeLowOffset + offset);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Uuid"/> struct.
        /// </summary>
        /// <param name="data">A 16-element byte span containing values with which to initialize the UUID.</param>
        private unsafe Uuid(ReadOnlySpan<byte> data)
        {
            fixed (byte* b = data)
            {
                _time_low = *(uint*)(b + TimeLowOffset);
                _time_mid = *(ushort*)(b + TimeMidOffset);
                _time_hi_and_version = *(ushort*)(b + TimeHiAndVersionOffset);
                _clock_seq_hi_and_reserved = *(b + ClockSeqHiAndReservedOffset);
                _clock_seq_low = *(b + ClockSeqLowOffset);
                _node_hi = *(ushort*)(b + NodeHiOffset);
                _node_low = *(uint*)(b + NodeLowOffset);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Uuid"/> struct.
        /// </summary>
        /// <param name="bytes">A 16-element byte array containing values with which to initialize the UUID.</param>
        /// <param name="time_hi_and_version">The high field of the timestamp multiplexed with the version number.</param>
        /// <param name="clock_seq_hi_and_reserved">The high field of the clock sequence multiplexed with the variant.</param>
        /// <remarks><para>Bytes 6-8 (zero indexed) of <paramref name="bytes"/> are ignored. <paramref name="clock_seq_hi_and_reserved"/> and <paramref name="time_hi_and_version"/> are used instead.</para></remarks>
        private unsafe Uuid(byte[] bytes, ushort time_hi_and_version, byte clock_seq_hi_and_reserved)
        {
            _time_hi_and_version = time_hi_and_version;
            _clock_seq_hi_and_reserved = clock_seq_hi_and_reserved;
            fixed (byte* b = bytes)
            {
                _time_low = *(uint*)(b + TimeLowOffset);
                _time_mid = *(ushort*)(b + TimeMidOffset);
                _clock_seq_low = *(b + ClockSeqLowOffset);
                _node_hi = *(ushort*)(b + NodeHiOffset);
                _node_low = *(uint*)(b + NodeLowOffset);
            }
        }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        /// <remarks>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Version</term>
        ///             <description>Description</description>
        ///         </listheader>
        ///         <item>
        ///             <term>1</term>
        ///             <description>The time-based version.</description>
        ///         </item>
        ///         <item>
        ///             <term>2</term>
        ///             <description>DCE Security version, with embedded POSIX UIDs.</description>
        ///         </item>
        ///         <item>
        ///             <term>3</term>
        ///             <description>The name-based version that uses MD5 hashing.</description>
        ///         </item>
        ///         <item>
        ///             <term>4</term>
        ///             <description>The randomly or pseudo-randomly generated version.</description>
        ///         </item>
        ///         <item>
        ///             <term>5</term>
        ///             <description>The name-based version that uses SHA-1 hashing.</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public int Version => (_time_hi_and_version & 0b1111_0000_0000_0000) >> VersionOffset;

        /// <summary>
        ///     Gets the timestamp.
        /// </summary>
        /// <value>
        ///     The timestamp.
        /// </value>
        /// <remarks>
        ///     <para>For UUID version 1, this is represented by Coordinated Universal Time (UTC) as a count of 100-nanosecond intervals since 00:00:00.00, 15 October 1582 (the date of Gregorian reform to the Christian calendar) in a 60-bit value.</para>
        ///     <para>For UUID version 3 or 5, the timestamp is a 60-bit value constructed from a name.</para>
        ///     <para>For UUID version 4, the timestamp is a randomly or pseudo-randomly generated 60-bit value.</para>
        /// </remarks>
        public long Timestamp => (long)(_time_low + ((ulong)_time_mid << 32) + (((ulong)_time_hi_and_version & TimeHighMask) << 48));

        /// <summary>
        ///     Gets the clock sequence.
        /// </summary>
        /// <value>
        ///     The clock sequence.
        /// </value>
        /// <remarks>
        ///     <para>For UUID version 1, the clock sequence is used to help avoid duplicates that could arise when the clock is set backwards in time or if the node ID changes.</para>
        ///     <para>For UUID version 3 or 5, the clock sequence is a 14-bit value constructed from a name.</para>
        ///     <para>For UUID version 4, clock sequence is a randomly or pseudo-randomly generated 14-bit value.</para>
        /// </remarks>
        public short ClockSequence => (short)(_clock_seq_low + ((_clock_seq_hi_and_reserved & ClockSeqHighMask) << 8));

        /// <summary>
        ///     Gets the node.
        /// </summary>
        /// <value>
        ///     The node.
        /// </value>
        /// <remarks>
        ///     <para>For UUID version 1, the node field consists of an IEEE 802 MAC address, usually the host address.  For systems with multiple IEEE 802 addresses, any available one can be used.  The lowest addressed octet (octet number 10) contains the global/local bit and the unicast/multicast bit, and is the first octet of the address transmitted on an 802.3 LAN.</para>
        ///     <para>For systems with no IEEE address, a randomly or pseudo-randomly generated value may be used.  The multicast bit must be set in such addresses, in order that they will never conflict with addresses obtained from network cards.</para>
        ///     <para>For UUID version 3 or 5, the node field is a 48-bit value constructed from a name.</para>
        ///     <para>For UUID version 4, the node field is a randomly or pseudo-randomly generated 48-bit value.</para>
        /// </remarks>
        public long Node => (long)(_node_low + ((ulong)_node_hi << 32));

        /// <summary>
        ///     Gets the debugger display text.
        /// </summary>
        /// <value>
        ///     The debugger display text.
        /// </value>
        /// <remarks><para>Gives nicer display for pre-defined UUIDs.</para></remarks>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                if (this == Nil)
                {
                    return nameof(Nil);
                }

                if (this == DNS)
                {
                    return nameof(DNS);
                }

                if (this == Url)
                {
                    return nameof(Url);
                }

                if (this == Oid)
                {
                    return nameof(Oid);
                }

                if (this == X500)
                {
                    return nameof(X500);
                }

                return ToString();
            }
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Guid"/> to <see cref="Uuid"/>.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static implicit operator Uuid(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            Array.Reverse(bytes, NodeHiOffset, sizeof(ushort));
            Array.Reverse(bytes, NodeLowOffset, sizeof(uint));
            return new Uuid(bytes);
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="Uuid"/> to <see cref="Guid"/>.
        /// </summary>
        /// <param name="uuid">The UUID.</param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        /// <exception cref="InvalidCastException">Only random (version 4) UUIDs can be converted to GUIDs.</exception>
        public static explicit operator Guid(Uuid uuid)
        {
            if (uuid.Version != 4)
            {
                throw new InvalidCastException("Only random (version 4) UUIDs can be converted to GUIDs.");
            }

            byte[] bytes = uuid.ToByteArray();
            Array.Reverse(bytes, TimeLowOffset, sizeof(uint));
            Array.Reverse(bytes, TimeMidOffset, sizeof(ushort));
            Array.Reverse(bytes, TimeHiAndVersionOffset, sizeof(ushort));
            return new Guid(bytes);
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(Uuid left, Uuid right) => left.Equals(right);

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator !=(Uuid left, Uuid right) => !left.Equals(right);

        /// <summary>
        ///     Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator >(Uuid left, Uuid right) => left.CompareTo(right) > 0;

        /// <summary>
        ///     Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator <(Uuid left, Uuid right) => left.CompareTo(right) < 0;

        /// <summary>
        ///     Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator >=(Uuid left, Uuid right) => left.CompareTo(right) >= 0;

        /// <summary>
        ///     Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator <=(Uuid left, Uuid right) => left.CompareTo(right) <= 0;

        /// <summary>
        ///     Creates a new MD5 name-based (version 3) UUID.
        /// </summary>
        /// <param name="namespaceID">The namespace UUID.</param>
        /// <param name="name">The name for the new UUID.</param>
        /// <returns>A new UUID.</returns>
        public static Uuid CreateMd5NameBased(in Uuid namespaceID, string name) => CreateNameBased<MD5CryptoServiceProvider>(namespaceID, name, 3);

        /// <summary>
        ///     Creates a new SHA1 name-based (version 5) UUID.
        /// </summary>
        /// <param name="namespaceID">The namespace UUID.</param>
        /// <param name="name">The name for the new UUID.</param>
        /// <returns>A new UUID.</returns>
        public static Uuid CreateSha1NameBased(in Uuid namespaceID, string name) => CreateNameBased<SHA1Managed>(namespaceID, name, 5);

        /// <summary>
        ///     Creates a new truly-random or pseudo-random (version 4) UUID.
        /// </summary>
        /// <returns>A new UUID.</returns>
        public static Uuid CreateRandom()
        {
            byte[] bytes = new byte[ByteLength];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetNonZeroBytes(bytes);
            }

            return new Uuid(
                bytes,
                (ushort)(0b0100_0000_0000_0000 | (BitConverter.ToUInt16(bytes, TimeHiAndVersionOffset) & TimeHighMask)),
                (byte)(0b1100_0000 | (bytes[ClockSeqHiAndReservedOffset] & ClockSeqHighMask)));
        }

        /// <summary>
        ///     Creates a new time-based (version 1) UUID.
        /// </summary>
        /// <returns>A new UUID.</returns>
        public static Uuid CreateTimeBased()
        {
            ulong timestamp = (ulong)(DateTimeOffset.UtcNow - new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero)).Ticks;
            PhysicalAddress? node = null;
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    node = networkInterface.GetPhysicalAddress();
                    break;
                }
            }

            if (node is null)
            {
                node = NetworkInterface.GetAllNetworkInterfaces()[NetworkInterface.LoopbackInterfaceIndex].GetPhysicalAddress();
            }

            byte[] clock_seq_bytes = new byte[2];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetNonZeroBytes(clock_seq_bytes);
            }

            ulong timeHiAndVersion = (timestamp >> 48) & TimeHighMask;
            timeHiAndVersion |= 1 << 12;
            ushort clock_seq = BitConverter.ToUInt16(clock_seq_bytes, 0);
            int clockSeqHiAndReserved = (clock_seq & 0b11_1111_0000_0000) >> 8;
            clockSeqHiAndReserved |= 0b1000_0000;
            return new Uuid(
                (uint)(timestamp & 0b1111_1111_1111_1111_1111_1111_1111_1111),
                (ushort)((timestamp >> 32) & 0b1111_1111_1111_1111),
                (ushort)timeHiAndVersion,
                (byte)clockSeqHiAndReserved,
                (byte)(clock_seq & 0b1111_1111),
                node.GetAddressBytes());
        }

        /// <summary>
        ///     Converts the string representation of a UUID to the equivalent <see cref="Uuid"/> structure.
        /// </summary>
        /// <param name="input">The UUID to convert.</param>
        /// <param name="result">The structure that will contain the parsed value. If the method returns <see langword="true"/>, <paramref name="result"/> contains a valid <see cref="Uuid"/>. If the method returns <see langword="false"/>, <paramref name="result"/> equals <see cref="Nil"/>.</param>
        /// <returns><see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.</returns>
        public static bool TryParse(string input, out Uuid result)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length != 36)
            {
                result = default;
                return false;
            }

            if (!uint.TryParse(input.Substring(0, 8), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint time_low) ||
                input[8] != '-' ||
                !ushort.TryParse(input.Substring(9, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ushort time_mid) ||
                input[13] != '-' ||
                !ushort.TryParse(input.Substring(14, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ushort time_hi_and_version) ||
                input[18] != '-' ||
                !byte.TryParse(input.Substring(19, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte clock_seq_hi_and_reserved) ||
                !byte.TryParse(input.Substring(21, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte clock_seq_low) ||
                input[23] != '-' ||
                !ushort.TryParse(input.Substring(24, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ushort node_hi) ||
                !uint.TryParse(input.Substring(28, 8), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint node_low))
            {
                result = default;
                return false;
            }

            result = new Uuid(time_low, time_mid, time_hi_and_version, clock_seq_hi_and_reserved, clock_seq_low, node_hi, node_low);
            return true;
        }

        /// <summary>
        ///     Create an <see cref="Uuid"/> from a <see cref="byte"/> array.
        /// </summary>
        /// <param name="data">The array to create the <see cref="Uuid"/> from.</param>
        /// <returns>A new <see cref="Uuid"/>.</returns>
        public static Uuid Create(byte[] data) => Create(data, 0);

        /// <summary>
        ///     Create an <see cref="Uuid"/> from a <see cref="byte"/> array starting at the specified index.
        /// </summary>
        /// <param name="data">The array to create the <see cref="Uuid"/> from.</param>
        /// <param name="startIndex">A 32-bit integer that represents the index in <paramref name="data"/> at which to start reading from.</param>
        /// <returns>A new <see cref="Uuid"/>.</returns>
        public static Uuid Create(byte[] data, int startIndex)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length < ByteLength)
            {
                throw new ArgumentException("data must have at least 16 bytes.", nameof(data));
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex must be zero or greater.");
            }

            if (startIndex >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex must be less than the length of data.");
            }

            if (startIndex + ByteLength >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex must be 16 less than the length of data.");
            }

            return new Uuid(data, startIndex);
        }

        /// <summary>
        ///     Create an <see cref="Uuid"/> from a <see cref="byte"/> span.
        /// </summary>
        /// <param name="data">The span to create the <see cref="Uuid"/> from.</param>
        /// <returns>A new <see cref="Uuid"/>.</returns>
        public static Uuid Create(ReadOnlySpan<byte> data)
        {
            if (data.Length != ByteLength)
            {
                throw new ArgumentException("data must have exactly 16 bytes.", nameof(data));
            }

            return new Uuid(data);
        }

        /// <summary>
        ///     Returns a 16-element byte array that contains the value of this instance.
        /// </summary>
        /// <returns>A 16-element byte array.</returns>
        /// <remarks><para>Note that the order of bytes returned is network order, regardless of the system.</para></remarks>
        public unsafe byte[] ToByteArray()
        {
            byte[] bytes = new byte[ByteLength];
            fixed (byte* b = bytes)
            {
                *(uint*)(b + TimeLowOffset) = _time_low;
                *(ushort*)(b + TimeMidOffset) = _time_mid;
                *(ushort*)(b + TimeHiAndVersionOffset) = _time_hi_and_version;
                *(b + ClockSeqHiAndReservedOffset) = _clock_seq_hi_and_reserved;
                *(b + ClockSeqLowOffset) = _clock_seq_low;
                *(ushort*)(b + NodeHiOffset) = _node_hi;
                *(uint*)(b + NodeLowOffset) = _node_low;
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes, TimeLowOffset, sizeof(uint));
                Array.Reverse(bytes, TimeMidOffset, sizeof(ushort));
                Array.Reverse(bytes, TimeHiAndVersionOffset, sizeof(ushort));
                Array.Reverse(bytes, NodeHiOffset, sizeof(ushort));
                Array.Reverse(bytes, NodeLowOffset, sizeof(uint));
            }

            return bytes;
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{_time_low:x8}-{_time_mid:x4}-{_time_hi_and_version:x4}-{_clock_seq_hi_and_reserved:x2}{_clock_seq_low:x2}-{_node_hi:x4}{_node_low:x8}";

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Uuid other) => _time_low == other._time_low && _time_mid == other._time_mid && _time_hi_and_version == other._time_hi_and_version && _clock_seq_hi_and_reserved == other._clock_seq_hi_and_reserved && _clock_seq_low == other._clock_seq_low && _node_hi == other._node_hi && _node_low == other._node_low;

        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if the specified <see cref="object" /> is equal to this instance; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj) => obj is Uuid uuid && Equals(uuid);

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(_time_low, _time_mid, _time_hi_and_version, _clock_seq_hi_and_reserved, _clock_seq_low, _node_hi);

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     <para>A value that indicates the relative order of the objects being compared. The return value has these meanings:</para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Value</term>
        ///             <description>Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Less than zero</term>
        ///             <description>This instance precedes <paramref name="other"/> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term>Zero</term>
        ///             <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
        ///         </item>
        ///         <item>
        ///             <term>Greater than zero</term>
        ///             <description>This instance follows <paramref name="other"/> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <remarks><para>Lexical ordering is not temporal ordering!.</para></remarks>
        public int CompareTo(Uuid other)
        {
            if (_time_low != other._time_low)
            {
                return _time_low.CompareTo(other._time_low);
            }

            if (_time_mid != other._time_mid)
            {
                return _time_mid.CompareTo(other._time_mid);
            }

            if (_time_hi_and_version != other._time_hi_and_version)
            {
                return _time_hi_and_version.CompareTo(other._time_hi_and_version);
            }

            if (_clock_seq_hi_and_reserved != other._clock_seq_hi_and_reserved)
            {
                return _clock_seq_hi_and_reserved.CompareTo(other._clock_seq_hi_and_reserved);
            }

            if (_clock_seq_low != other._clock_seq_low)
            {
                return _clock_seq_low.CompareTo(other._clock_seq_low);
            }

            if (_node_hi != other._node_hi)
            {
                return _node_hi.CompareTo(other._node_hi);
            }

            if (_node_low != other._node_low)
            {
                return _node_low.CompareTo(other._node_low);
            }

            return 0;
        }

        /// <summary>
        ///     Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or <see langword="null"/>.</param>
        /// <returns>
        ///     <para>A signed number indicating the relative values of this instance and <paramref name="obj"/>.</para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Return value</term>
        ///             <description>Description</description>
        ///         </listheader>
        ///         <item>
        ///             <term>A negative integer</term>
        ///             <description>This instance is less than <paramref name="obj"/>.</description>
        ///         </item>
        ///         <item>
        ///             <term>Zero</term>
        ///             <description>This instance is equal to <paramref name="obj"/>.</description>
        ///         </item>
        ///         <item>
        ///             <term>A positive integer</term>
        ///             <description>This instance is greater than <paramref name="obj"/>, or <paramref name="obj"/> is <see langword="null"/>.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="Uuid"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is Uuid other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException("obj must be a UUID.", nameof(obj));
        }

        /// <summary>
        ///     Create a new name-based (version 3 or 5) UUID.
        /// </summary>
        /// <typeparam name="TAlg">The type of hashing algorithm to use.</typeparam>
        /// <param name="namespaceID">The namespace UUID.</param>
        /// <param name="name">The name for the new UUID.</param>
        /// <param name="version">The version.</param>
        /// <returns>A new UUID.</returns>
        private static Uuid CreateNameBased<TAlg>(in Uuid namespaceID, string name, byte version) where TAlg : HashAlgorithm, new()
        {
            byte[] hash;
            using (TAlg alg = new TAlg())
            {
                alg.Initialize();
                byte[] namespaceBytes = namespaceID.ToByteArray();
                byte[] nameBytes = Encoding.ASCII.GetBytes(name);
                alg.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, namespaceBytes, 0);
                alg.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = alg.Hash;
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(hash, TimeLowOffset, sizeof(uint));
                Array.Reverse(hash, TimeMidOffset, sizeof(ushort));
                Array.Reverse(hash, TimeHiAndVersionOffset, sizeof(ushort));
                Array.Reverse(hash, NodeHiOffset, sizeof(ushort));
                Array.Reverse(hash, NodeLowOffset, sizeof(uint));
            }

            return new Uuid(
                hash,
                (ushort)((version << VersionOffset) | (BitConverter.ToUInt16(hash, TimeHiAndVersionOffset) & TimeHighMask)),
                (byte)(0b1000_0000 | (hash[ClockSeqHiAndReservedOffset] & ClockSeqHighMask)));
        }
    }
}
