using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FeedlyOpmlExport.Functions
{
    public static class RefreshFeedlyAuthToken
    {
        // These will be read from a settings file, or environment variables, which in production will point to the key vault.
        public static string userId = System.Environment.GetEnvironmentVariable("feedly-user-id");
        public static string accessToken = System.Environment.GetEnvironmentVariable("feedly-access-token");
        public static string refreshToken = System.Environment.GetEnvironmentVariable("feedly-refresh-token");

        [FunctionName("RefreshFeedlyAuthToken")]
        public static void Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            log.LogInformation($"UserId: {userId}, Access Token: {accessToken}, Refresh token: {refreshToken}");
        }
    }
}
