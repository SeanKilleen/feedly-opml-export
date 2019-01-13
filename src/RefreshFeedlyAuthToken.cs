using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FeedlyOpmlExport.Functions
{
    public static class RefreshFeedlyAuthToken
    {
        // These will be read from a settings file, or environment variables, which in production will point to the key vault.
        public static string userId = System.Environment.GetEnvironmentVariable("feedly-user-id");
        public static string accessToken = System.Environment.GetEnvironmentVariable("feedly-access-token");
        public static string refreshToken = System.Environment.GetEnvironmentVariable("feedly-refresh-token");

        public const string FEEDLY_BASE_URL = "https://cloud.feedly.com/v3/";

        [FunctionName("RefreshFeedlyAuthToken")]
        public static async Task Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            log.LogInformation($"UserId: {userId}, Access Token: {accessToken}, Refresh token: {refreshToken}");

            var client = new HttpClient { BaseAddress = new Uri(FEEDLY_BASE_URL) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var request = new FeedlyRefreshRequest(userId, accessToken, refreshToken);
            var response = await client.PostAsJsonAsync("auth/token", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                log.LogWarning($"Response failed. Status {response.StatusCode}, Reason {response.ReasonPhrase}");
                log.LogWarning($"Error content: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            log.LogInformation("Awesome! It worked!");
            var content = await response.Content.ReadAsStringAsync();

            log.LogInformation("Response content:");
            log.LogInformation(content);
        }
    }

    public class FeedlyRefreshRequest
    {
        public string refresh_token {get;}
        public string client_id {get;}
        public string client_secret {get;}
        public const string grant_type = "refresh_token";

        public FeedlyRefreshRequest(string id, string secret, string refreshToken)
        {
            client_id = id;
            client_secret = secret;
            refresh_token = refreshToken;
        }        
    }
}
