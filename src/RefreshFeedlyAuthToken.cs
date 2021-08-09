using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FeedlyOpmlExport.Functions.Utilities;

namespace FeedlyOpmlExport.Functions
{
    // ReSharper disable once UnusedMember.Global -- azure functions calls this
    public static class RefreshFeedlyAuthToken
    {
        // These will be read from a settings file, or environment variables, which in production will point to the key vault.
        private static readonly string userId = Environment.GetEnvironmentVariable("feedly-user-id");
        private static readonly string refreshToken = Environment.GetEnvironmentVariable("feedly-refresh-token");


        [FunctionName("RefreshFeedlyAuthToken")]
        // ReSharper disable once UnusedParameter.Global
        public static async Task Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"RefreshFeedlyAuthToken function executed at: {DateTime.Now}");
            log.LogInformation($"UserId: {userId}");

            log.LogInformation("Getting current access token contents from key vault");
            var accessToken = await KeyVaultManager.GetFeedlyAccessToken();

            log.LogInformation("Getting refreshed access token from Feedly API");
            var feedlyResponse = await FeedlyManager.RefreshFeedlyAccessToken(accessToken, log, refreshToken);

            log.LogInformation("Setting the secret in the key vault");
            await KeyVaultManager.UpdateFeedlyAccessToken(feedlyResponse.access_token);

            log.LogInformation("Successfully updated token in the key vault");
        }

    }
}
