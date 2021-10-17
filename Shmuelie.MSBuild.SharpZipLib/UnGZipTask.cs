using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Build.Utilities;

namespace Shmuelie.MSBuild.SharpZipLib
{
    public sealed class UnGZipTask : ExtractTask
    {
        protected override bool Extract(string sourceFile, string destinationFolder)
        {
            FileStream? sourceStream = null;
            GZipInputStream? gzipStream = null;
            FileStream? destinationStream = null;
            try
            {
                sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                gzipStream = new GZipInputStream(sourceStream);
                string destinationFile = Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(sourceFile));
                destinationStream = new FileStream(destinationFile, Overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.None);
                Log.LogMessage("Extracting {0} to {1}", sourceFile, destinationFile);
                gzipStream.CopyTo(destinationStream, 4096);
                ExtractedFiles = new TaskItem[] { new TaskItem(destinationFile) };
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
            finally
            {
                destinationStream?.Dispose();
                gzipStream?.Dispose();
                sourceStream?.Dispose();
            }
        }
    }
}
