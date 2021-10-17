using System.IO;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Shmuelie.MSBuild.SharpZipLib
{
    /// <summary>
    ///     A MSBuild <see cref="BuildTask"/> that extracts data.
    /// </summary>
    /// <seealso cref="BuildTask"/>
    public abstract class ExtractTask : BuildTask, ICancelableTask
    {
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        internal const int DefaultCopyBufferSize = 81920;

        /// <summary>
        /// Stores a <see cref="CancellationTokenSource"/> used for cancellation.
        /// </summary>
        protected readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        /// <summary>
        /// Gets or sets a <see cref="ITaskItem"/> containing the paths to archive files to extract.
        /// </summary>
        [Required]
        public TaskItem? SourceFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a <see cref="ITaskItem"/> with a destination folder path to extract the files to.
        /// </summary>
        [Required]
        public TaskItem? DestinationFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether existing files should be overwritten.
        /// </summary>
        [Required]
        public bool Overwrite
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the extracted files.
        /// </summary>
        [Output]
        public TaskItem[]? ExtractedFiles
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public void Cancel()
        {
            cancellationToken.Cancel();
        }

        /// <inheritdoc />
        public sealed override bool Execute()
        {
            string? source = SourceFile?.ItemSpec;
            if (string.IsNullOrWhiteSpace(source))
            {
                Log.LogError($"{nameof(SourceFile)} cannot be null or whitespace only.");
                return false;
            }
            string sourceFile = Path.GetFullPath(source);
            if (!File.Exists(sourceFile))
            {
                Log.LogError($"{nameof(SourceFile)} must exist.");
                return false;
            }
            string? destination = DestinationFolder?.ItemSpec;
            if (string.IsNullOrWhiteSpace(destination))
            {
                Log.LogError($"{nameof(DestinationFolder)} cannot null or whitespace only");
                return false;
            }
            string destinationFolder = Path.GetFullPath(destination);
            if (!Directory.Exists(destinationFolder))
            {
                Log.LogError($"{nameof(DestinationFolder)} must exist.");
                return false;
            }
            return Extract(sourceFile, destinationFolder);
        }

        /// <summary>
        /// Extracts files to a directory.
        /// </summary>
        /// <param name="sourceFile">Archive to extract.</param>
        /// <param name="destinationFolder">Directory to extract files to.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        protected abstract bool Extract(string sourceFile, string destinationFolder);
    }
}
