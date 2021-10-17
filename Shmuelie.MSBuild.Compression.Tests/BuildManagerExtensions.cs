using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Build.Execution;

namespace Shmuelie.MSBuild.Compression.Tests
{
    internal static class BuildManagerExtensions
    {
        public static Task<BuildResult> BuildRequestAsync(this BuildManager @this, BuildRequestData buildRequestData)
        {
            TaskCompletionSource<BuildResult> taskCompletionSource = new TaskCompletionSource<BuildResult>();
            BuildSubmission submission = @this.PendBuildRequest(buildRequestData);
            submission.ExecuteAsync(BuildRequestAsyncCompleted, taskCompletionSource);
            return taskCompletionSource.Task;
        }

        private static void BuildRequestAsyncCompleted(BuildSubmission submission)
        {
            BuildResult buildResult = submission.BuildResult;
            TaskCompletionSource<BuildResult> taskCompletionSource = ((TaskCompletionSource<BuildResult>)submission.AsyncContext);
            if (buildResult.OverallResult == BuildResultCode.Success)
            {
                taskCompletionSource.SetResult(buildResult);
            }
            else if (buildResult.Exception != null)
            {
                taskCompletionSource.SetException(buildResult.Exception);
            }
            else
            {
                Exception exception = new Exception("No Idea What Happened");
                exception.Data.Add("BuildResult", buildResult);
                taskCompletionSource.SetException(exception);
            }
        }

        public static Task<BuildResult> BuildProjectAsync(this BuildManager @this, string projectFullPath, string configuration, string platform, string outputPath, string toolsVersion, string[] targetsToBuild, HostServices hostServices)
        {
            return @this.BuildRequestAsync(new BuildRequestData(projectFullPath, new Dictionary<string, string>
            {
                { "Configuration", configuration },
                { "Platform", platform },
                { "OutputPath", outputPath }
            }, toolsVersion, targetsToBuild, null));
        }
    }
}
