// <copyright file="UuidConverter.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Globalization;

namespace Shmuelie.Common.Converters
{
    /// <summary>
    ///     Provides a type converter to convert <see cref="Uuid"/> objects to and from various other representations.
    /// </summary>
    /// <remarks><para>This converter can only convert an UUID object to and from a string.</para></remarks>
    /// <seealso cref="TypeConverter" />
    /// <seealso cref="Uuid"/>
    public sealed class UuidConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string input && Uuid.TryParse(input, out Uuid result))
            {
                return result;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
