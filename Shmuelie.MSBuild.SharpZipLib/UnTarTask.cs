using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Build.Utilities;

namespace Shmuelie.MSBuild.SharpZipLib
{
    /// <summary>
    ///     Extract files from a TAR archive.
    /// </summary>
    /// <seealso cref="ExtractTask"/>
    public sealed class UnTarTask : ExtractTask
    {
        /// <inheritdoc/>
        protected override bool Extract(string sourceFile, string destinationFolder)
        {
            FileStream? stream = null;
            TarInputStream? input = null;
            try
            {
                stream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                input = new TarInputStream(stream, Encoding.UTF8);
                Log.LogMessage("Extracting {0} to {1}", sourceFile, destinationFolder);
                TarEntry? tarEntry = null;
                List<string> extractedFiles = new List<string>();
                while ((tarEntry = input.GetNextEntry()) != null)
                {
                    if (tarEntry.IsDirectory)
                    {
                        continue;
                    }
                    string name = tarEntry.Name;
                    if (Path.IsPathRooted(name))
                    {
                        name = name.Substring(Path.GetPathRoot(name).Length);
                    }
                    Log.LogMessage("Extracting file {0}", name);
                    string outName = Path.Combine(destinationFolder, name);
                    string directoryName = Path.GetDirectoryName(outName);
                    Directory.CreateDirectory(directoryName);
                    using (FileStream outStr = new FileStream(outName, Overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.None))
                    {
                        input.CopyEntryContents(outStr);
                    }
                    DateTime myDt = DateTime.SpecifyKind(tarEntry.ModTime, DateTimeKind.Utc);
                    File.SetLastWriteTime(outName, myDt);
                    extractedFiles.Add(outName);
                }
                ExtractedFiles = extractedFiles.Select(s => new TaskItem(s)).ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
            finally
            {
                input?.Dispose();
                stream?.Dispose();
            }
        }
    }
}
