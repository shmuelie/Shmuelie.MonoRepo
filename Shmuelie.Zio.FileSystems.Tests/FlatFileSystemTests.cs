using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Zio;

namespace Shmuelie.Zio.FileSystems.Tests
{
    [TestFixture]
    [TestOf(typeof(FlatFileSystem))]
    public class FlatFileSystemTests
    {
        private static readonly UPath[] paths = { "/a.txt", "/a/b.txt", "/a/c.text" };

        private class TestFlatFS : FlatFileSystem
        {
            protected override IEnumerable<UPath> Paths { get; }

            public TestFlatFS(IEnumerable<UPath> paths) => Paths = paths;

            protected override UPath ConvertPathFromInternalImpl(string innerPath) => throw new NotImplementedException();
            protected override string ConvertPathToInternalImpl(UPath path) => throw new NotImplementedException();
            protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite) => throw new NotImplementedException();
            protected override void CreateDirectoryImpl(UPath path) => throw new NotImplementedException();
            protected override void DeleteDirectoryImpl(UPath path, bool isRecursive) => throw new NotImplementedException();
            protected override void DeleteFileImpl(UPath path) => throw new NotImplementedException();
            protected override FileAttributes GetAttributesImpl(UPath path) => throw new NotImplementedException();
            protected override DateTime GetCreationTimeImpl(UPath path) => throw new NotImplementedException();
            protected override long GetFileLengthImpl(UPath path) => throw new NotImplementedException();
            protected override DateTime GetLastAccessTimeImpl(UPath path) => throw new NotImplementedException();
            protected override DateTime GetLastWriteTimeImpl(UPath path) => throw new NotImplementedException();
            protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath) => throw new NotImplementedException();
            protected override void MoveFileImpl(UPath srcPath, UPath destPath) => throw new NotImplementedException();
            protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share) => throw new NotImplementedException();
            protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors) => throw new NotImplementedException();
            protected override void SetAttributesImpl(UPath path, FileAttributes attributes) => throw new NotImplementedException();
            protected override void SetCreationTimeImpl(UPath path, DateTime time) => throw new NotImplementedException();
            protected override void SetLastAccessTimeImpl(UPath path, DateTime time) => throw new NotImplementedException();
            protected override void SetLastWriteTimeImpl(UPath path, DateTime time) => throw new NotImplementedException();
            protected override IFileSystemWatcher WatchImpl(UPath path) => throw new NotImplementedException();
        }

        [TestCaseSource(typeof(TestData), nameof(TestData.DirectoryExistsTestData))]
        public bool DirectoryExistsTest(UPath testPath) => new TestFlatFS(paths).DirectoryExists(testPath);

        [Test]
        public void DirectoryExistsTest() => Assert.That(() => new TestFlatFS(paths).DirectoryExists(UPath.Empty), Throws.ArgumentException);

        [TestCaseSource(typeof(TestData), nameof(TestData.FileExistsTestData))]
        public bool FileExistsTest(UPath testPath) => new TestFlatFS(paths).FileExists(testPath);

        [Test]
        public void FileExistsTest() => Assert.That(() => new TestFlatFS(paths).FileExists(UPath.Empty), Throws.ArgumentException);

        [TestCaseSource(typeof(TestData), nameof(TestData.EnumeratePathsData))]
        public int EnumeratePaths(UPath testPath, string searchPattern, SearchOption searchOption, SearchTarget searchTarget) => new TestFlatFS(paths).EnumeratePaths(testPath, searchPattern, searchOption, searchTarget).Count();
    }
}
