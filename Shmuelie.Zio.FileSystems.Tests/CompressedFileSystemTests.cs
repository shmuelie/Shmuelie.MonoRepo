using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using Zio;

namespace Shmuelie.Zio.FileSystems.Tests
{
#nullable disable
    [TestFixture]
    [TestOf(typeof(CompressedFileSystem))]
    public class CompressedFileSystemTests
    {
        private static readonly string ArchivePath = Path.Combine(Path.GetDirectoryName(typeof(CompressedFileSystemTests).Assembly.Location), "TestArchive.zip");
        private static readonly string ArchiveTempPath = Path.Combine(Path.GetDirectoryName(typeof(CompressedFileSystemTests).Assembly.Location), "TestArchive.tmp.zip");
        private IFileSystem fileSystem;

        [SetUp]
        [MemberNotNull(nameof(fileSystem))]
        public void PreTest()
        {
            File.Copy(ArchivePath, ArchiveTempPath, true);
            fileSystem = new CompressedFileSystem(ZipFile.Open(ArchiveTempPath, ZipArchiveMode.Update));
        }

        [TearDown]
        public void PostTest()
        {
            fileSystem?.Dispose();
            File.Delete(ArchiveTempPath);
        }

        [TestCaseSource(typeof(TestData), nameof(TestData.DirectoryExistsTestData))]
        public bool DirectoryExistsTest(UPath testPath) => fileSystem.DirectoryExists(testPath);
        [Test]
        public void DirectoryExistsTest() => Assert.That(() => fileSystem.DirectoryExists(UPath.Empty), Throws.ArgumentException);
        [TestCaseSource(typeof(TestData), nameof(TestData.FileExistsTestData))]
        public bool FileExistsTest(UPath testPath) => fileSystem.FileExists(testPath);
        [Test]
        public void FileExistsTest() => Assert.That(() => fileSystem.FileExists(UPath.Empty), Throws.ArgumentException);
        [TestCaseSource(typeof(TestData), nameof(TestData.EnumeratePathsData))]
        public int EnumeratePaths(UPath testPath, string searchPattern, SearchOption searchOption, SearchTarget searchTarget) => fileSystem.EnumeratePaths(testPath, searchPattern, searchOption, searchTarget).Count();

        [Test]
        public void DeleteFile()
        {
            fileSystem.DeleteFile(UPath.Root / "a.txt");
            Assert.That(fileSystem.FileExists(UPath.Root / "a.txt"), Is.False);
        }
    }
}
