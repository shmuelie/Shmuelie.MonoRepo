using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using NUnit.Framework;
using Zio;
using Zio.FileSystems;

namespace Shmuelie.Zio.Glob.Tests
{
    [TestFixture]
    [TestOf(typeof(MatcherExtensions))]
    public class MatcherTests
    {
        [Test]
        public void Test()
        {
            using (MemoryFileSystem fileSystem = new MemoryFileSystem())
            {
                fileSystem.CreateFile("/one.txt").Dispose();
                fileSystem.CreateDirectory("/dir/");
                fileSystem.CreateFile("/dir/two.txt").Dispose();
                fileSystem.CreateFile("/three.png").Dispose();
                fileSystem.CreateFile("/dir/four.png").Dispose();

                Matcher matcher = new Matcher();
                matcher.AddInclude("*.txt");
                PatternMatchingResult result = matcher.Execute(new DirectoryInfo(fileSystem.GetDirectoryEntry(UPath.Root)));
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Property(nameof(PatternMatchingResult.HasMatches)).True);
                FilePatternMatch[] files = result.Files.ToArray();
                Assert.That(files, Is.Not.Null);
                Assert.That(files, Has.Length.EqualTo(1));
                Assert.That(files[0].Path, Is.EqualTo("one.txt"));
                Assert.That(matcher.GetResultsInFullPath(fileSystem, UPath.Root).First(), Is.EqualTo(UPath.Root / "one.txt"));
            }
        }
    }
}
