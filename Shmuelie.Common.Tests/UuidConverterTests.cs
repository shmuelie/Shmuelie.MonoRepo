using System;
using System.ComponentModel;
using NUnit.Framework;
using Shmuelie.Common;
using Shmuelie.Common.Converters;

namespace Shmueli.Common.Tests
{
    [TestFixture]
    public class UuidConverterTests
    {
        [Test]
        public void Connected()
        {
            Assert.That(TypeDescriptor.GetConverter(typeof(Uuid)), Is.TypeOf<UuidConverter>());
            Assert.That(TypeDescriptor.GetConverter(new Uuid()), Is.TypeOf<UuidConverter>());
        }

        [Test]
        [TestCase(typeof(string), ExpectedResult = true)]
        [TestCase(typeof(int), ExpectedResult = false)]
        [TestCase(typeof(object), ExpectedResult = false)]
        [TestCase(typeof(byte[]), ExpectedResult = false)]
        public bool CanConvertFrom(Type type) => new UuidConverter().CanConvertFrom(type);

        [Test]
        [TestCase(typeof(string), ExpectedResult = true)]
        [TestCase(typeof(int), ExpectedResult = false)]
        [TestCase(typeof(object), ExpectedResult = false)]
        [TestCase(typeof(byte[]), ExpectedResult = false)]
        public bool CanConvertTo(Type type) => new UuidConverter().CanConvertTo(type);

        [Test]
        public void ConvertFrom()
        {
            UuidConverter converter = new UuidConverter();
            object result = converter.ConvertFrom("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<Uuid>());
            Assert.That(result, Is.EqualTo(Uuid.DNS));
            Assert.That(() => converter.ConvertFrom(null), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => converter.ConvertFrom(""), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => converter.ConvertFrom(1), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void ConvertTo()
        {
            UuidConverter converter = new UuidConverter();
            object result = converter.ConvertTo(Uuid.DNS, typeof(string));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<string>());
            Assert.That(result, Is.EqualTo("6ba7b810-9dad-11d1-80b4-00c04fd430c8"));
            Assert.That(() => converter.ConvertTo(Uuid.DNS, typeof(int)), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => converter.ConvertTo(Uuid.DNS, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(converter.ConvertTo("", typeof(string)), Is.Empty);
        }
    }
}
