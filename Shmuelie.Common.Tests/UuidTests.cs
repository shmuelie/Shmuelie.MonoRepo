using System;
using NUnit.Framework;

namespace Shmuelie.Common.Tests
{
    [TestFixture]
    public class UuidTests
    {
        [Test]
        public void Nil()
        {
            Assert.That(new Uuid(), Is.EqualTo(Uuid.Nil));
        }

        [Test]
        public void Parse()
        {
            string a = "25658646-0b6d-497e-9f3b-63e6cfed115e";
            Assert.That(Uuid.TryParse(a, out Uuid result), Is.True);
            Assert.That(result.ToString(), Is.EqualTo(a));
            Assert.That(Uuid.TryParse(string.Empty, out Uuid empty), Is.False);
            Assert.That(empty, Is.EqualTo(Uuid.Nil));
            Assert.That(Uuid.TryParse(a.Replace("-", ""), out Uuid invalid), Is.False);
            Assert.That(invalid, Is.EqualTo(Uuid.Nil));
            Assert.That(Uuid.TryParse(a.Replace("-", "_"), out Uuid invalid2), Is.False);
            Assert.That(invalid2, Is.EqualTo(Uuid.Nil));
        }

        [Test]
        public void Version4()
        {
            Uuid uuid = Uuid.CreateRandom();
            TestContext.WriteLine("UUID: " + uuid);
            Assert.That(uuid.Version, Is.EqualTo(4));
        }

        [Test]
        public void Version1()
        {
            Uuid uuid = Uuid.CreateTimeBased();
            TestContext.WriteLine("UUID: " + uuid);
            Assert.That(uuid.Version, Is.EqualTo(1));
        }

        [Test]
        public void Version3()
        {
            Uuid uuid = Uuid.CreateMd5NameBased(Uuid.Nil, "Hello World");
            TestContext.WriteLine("UUID: " + uuid);
            Assert.That(uuid.Version, Is.EqualTo(3));
        }

        [Test]
        public void Version5()
        {
            Uuid uuid = Uuid.CreateSha1NameBased(Uuid.Nil, "Hello World");
            TestContext.WriteLine("UUID: " + uuid);
            Assert.That(uuid.Version, Is.EqualTo(5));
        }

        [Test]
        public void Equality()
        {
            string a = "5eede591-66a8-4cd0-b0d0-8154732a1d9e";
            Assert.That(Uuid.TryParse(a, out Uuid result), Is.True);
            Assert.That(result != Uuid.Nil, Is.True);
            Assert.That(Uuid.TryParse(a, out Uuid other), Is.True);
            Assert.That(result == other, Is.True);
            Assert.That(Uuid.Nil.Equals(new object()), Is.False);
            Assert.That(result.Equals((object)other), Is.True);
        }

        [Test]
        public void Dns()
        {
            string str = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";
            Assert.That(Uuid.DNS.ToString(), Is.EqualTo(str));
            Assert.That(Uuid.DNS.Version, Is.EqualTo(1));
        }

        [Test]
        public void Url()
        {
            string str = "6ba7b811-9dad-11d1-80b4-00c04fd430c8";
            Assert.That(Uuid.Url.ToString(), Is.EqualTo(str));
            Assert.That(Uuid.Url.Version, Is.EqualTo(1));
        }

        [Test]
        public void Oid()
        {
            string str = "6ba7b812-9dad-11d1-80b4-00c04fd430c8";
            Assert.That(Uuid.Oid.ToString(), Is.EqualTo(str));
            Assert.That(Uuid.Oid.Version, Is.EqualTo(1));
        }

        [Test]
        public void X500()
        {
            string str = "6ba7b814-9dad-11d1-80b4-00c04fd430c8";
            Assert.That(Uuid.X500.ToString(), Is.EqualTo(str));
            Assert.That(Uuid.X500.Version, Is.EqualTo(1));
        }

        [Test]
        public void CompareTo()
        {
            Assert.That(Uuid.TryParse("7d444840-9dc0-11d1-b245-5ffdce74fad2", out Uuid uuid), Is.True);
            Assert.That(uuid, Is.GreaterThan(Uuid.DNS));
            Assert.That(uuid.CompareTo(Uuid.DNS), Is.Positive);
            Assert.That(uuid.CompareTo((object)Uuid.DNS), Is.Positive);
            Assert.That(Uuid.DNS.CompareTo(uuid), Is.Negative);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.That(uuid.CompareTo(null), Is.Positive);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.That(() => uuid.CompareTo(new object()), Throws.ArgumentException);
            Assert.That(uuid > Uuid.DNS);
            Assert.That(uuid >= Uuid.DNS);
            Assert.That(Uuid.Nil >= new Uuid());
            Assert.That(Uuid.DNS < uuid);
            Assert.That(Uuid.DNS <= uuid);
            Assert.That(Uuid.Nil <= new Uuid());

            Assert.That(Uuid.TryParse("7d444840-9dc0-11d1-b245-5ffdce74fad2", out Uuid a), Is.True);
            Assert.That(Uuid.TryParse("7d444840-9dc0-11d1-b245-5ffdce74fad3", out Uuid b), Is.True);
            Assert.That(Uuid.TryParse("7d444840-9dc0-11d1-b246-5ffdce74fad2", out Uuid c), Is.True);
            Assert.That(Uuid.TryParse("7d444840-9dc0-11d2-b245-5ffdce74fad2", out Uuid d), Is.True);
            Assert.That(Uuid.TryParse("7d444840-9dc0-11d1-b245-6ffdce74fad2", out Uuid e), Is.True);
            Assert.That(Uuid.TryParse("7d444840-9dc1-11d1-b245-6ffdce74fad2", out Uuid f), Is.True);
            Assert.That(Uuid.TryParse("7d444840-9dc0-11d1-c245-5ffdce74fad2", out Uuid h), Is.True);
            Assert.That(a.CompareTo(b), Is.Negative);
            Assert.That(a.CompareTo(c), Is.Negative);
            Assert.That(a.CompareTo(d), Is.Negative);
            Assert.That(a.CompareTo(e), Is.Negative);
            Assert.That(a.CompareTo(f), Is.Negative);
            Assert.That(a.CompareTo(h), Is.Negative);
        }

        [Test]
        public void FromGuid()
        {
            Guid guid = Guid.NewGuid();
            Uuid uuid = guid;
            Assert.That(uuid.Version, Is.EqualTo(4));
            Assert.That(uuid.ToString(), Is.EqualTo(guid.ToString()));
        }

        [Test]
        public void ToGuid()
        {
            Uuid uuid = Uuid.CreateRandom();
            Guid guid = (Guid)uuid;
            Assert.That(guid.ToString(), Is.EqualTo(uuid.ToString()));
            Assert.That(() => guid = (Guid)Uuid.CreateTimeBased(), Throws.TypeOf<InvalidCastException>());
        }

        [Test]
        public void Md5FromNameSpace()
        {
            string str = "5df41881-3aed-3515-88a7-2f4a814cf09e";
            Uuid uuid = Uuid.CreateMd5NameBased(Uuid.DNS, "www.example.com");
            Assert.That(uuid.Version, Is.EqualTo(3));
            Assert.That(uuid.ToString(), Is.EqualTo(str));
            Assert.That(uuid.Node, Is.EqualTo(0x2f4a814cf09e));
            Assert.That(uuid.ClockSequence, Is.EqualTo(0x8a7));
            Assert.That(uuid.Timestamp, Is.EqualTo(0x5153aed5df41881));
        }

        [Test]
        public void Sha1FromNameSpace()
        {
            string str = "2ed6657d-e927-568b-95e1-2665a8aea6a2";
            Uuid uuid = Uuid.CreateSha1NameBased(Uuid.DNS, "www.example.com");
            Assert.That(uuid.Version, Is.EqualTo(5));
            Assert.That(uuid.ToString(), Is.EqualTo(str));
        }

        [Test]
        public void ToByteArray()
        {
            byte[] bytes = Uuid.Nil.ToByteArray();
            Assert.That(bytes, Has.Length.EqualTo(16));
            Assert.That(bytes, Has.All.EqualTo(0));
        }
    }
}
