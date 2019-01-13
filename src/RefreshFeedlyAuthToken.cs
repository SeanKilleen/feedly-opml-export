using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace FeedlyOpmlExport.Functions
{
    public static class RefreshFeedlyAuthToken
    {
        // These will be read from a settings file, or environment variables, which in production will point to the key vault.
        private static readonly string userId = System.Environment.GetEnvironmentVariable("feedly-user-id");
        private static readonly string refreshToken = System.Environment.GetEnvironmentVariable("feedly-refresh-token");

        private const string FEEDLY_BASE_URL = "https://cloud.feedly.com/v3/";

        [FunctionName("RefreshFeedlyAuthToken")]
        public static async Task Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            log.LogInformation($"UserId: {userId}, Refresh token: {refreshToken}");

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            
            log.LogInformation("Getting access token contents from keyvault");
            var accessToken = await kv.GetSecretAsync("https://feedly-export-keyvault.vault.azure.net", "feedly-access-token", CancellationToken.None);
            
            var client = new HttpClient { BaseAddress = new Uri(FEEDLY_BASE_URL) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);

            var request = new FeedlyRefreshRequest(refreshToken);
            var response = await client.PostAsJsonAsync("auth/token", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                log.LogWarning($"Response failed. Status {response.StatusCode}, Reason {response.ReasonPhrase}");
                log.LogWarning($"Error content: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            log.LogInformation("Awesome! It worked!");

            var feedlyResponse = JsonConvert.DeserializeObject<FeedlyRefreshResponse>(await response.Content.ReadAsStringAsync());

            log.LogInformation($"TODO Remove -- new Auth token: {feedlyResponse.access_token}, plan: {feedlyResponse.plan}");

            log.LogInformation("Setting the secret in the keyvault");
            await kv.SetSecretAsync("https://feedly-export-keyvault.vault.azure.net", "feedly-access-token", feedlyResponse.access_token);

            log.LogInformation($"Successfully updated token from {accessToken.Value} to {feedlyResponse.access_token}");
        }
    }

    public class FeedlyRefreshRequest
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable InconsistentNaming
        public string refresh_token { get; }
        public string client_id =>  "feedlydev";
        public string client_secret => "feedlydev";
        public string grant_type => "refresh_token";
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming

        public FeedlyRefreshRequest(string refreshToken)
        {
            refresh_token = refreshToken;
        }        
    }

    public class FeedlyRefreshResponse
    {
        public string id { get; set; }
        public string access_token { get; set; }
        public long expires_in { get; set; }
        public string token_type { get; set; }
        public string plan { get; set; }

    }
}
