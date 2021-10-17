using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Zio;

#nullable disable
namespace Shmuelie.Zio.FileSystems.Tests
{
    public static class TestData
    {
        public static IEnumerable<TestCaseData> DirectoryExistsTestData()
        {
            yield return new TestCaseData(new UPath("/a")).Returns(true);
            yield return new TestCaseData(new UPath("/b")).Returns(false);
            yield return new TestCaseData(new UPath("/a/b")).Returns(false);
            yield return new TestCaseData(new UPath("/a/b.txt")).Returns(false);
            yield return new TestCaseData(UPath.Root).Returns(true);
            yield return new TestCaseData(new UPath(null)).Returns(false);
        }

        public static IEnumerable<TestCaseData> FileExistsTestData()
        {
            yield return new TestCaseData(new UPath("/a")).Returns(false);
            yield return new TestCaseData(new UPath("/b")).Returns(false);
            yield return new TestCaseData(new UPath("/a/c.text")).Returns(true);
            yield return new TestCaseData(new UPath("/a/b.txt")).Returns(true);
            yield return new TestCaseData(new UPath(null)).Returns(false);
        }

        public static IEnumerable<TestCaseData> EnumeratePathsData()
        {
            yield return new TestCaseData(UPath.Root, "*.txt", SearchOption.AllDirectories, SearchTarget.File).Returns(2);
            yield return new TestCaseData(UPath.Root, "*.txt", SearchOption.TopDirectoryOnly, SearchTarget.File).Returns(1);
        }
    }
}
