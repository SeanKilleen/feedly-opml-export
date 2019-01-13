using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json;

namespace FeedlyOpmlExport.Functions
{
    // ReSharper disable once UnusedMember.Global -- azure functions calls this
    public static class RefreshFeedlyAuthToken
    {
        // These will be read from a settings file, or environment variables, which in production will point to the key vault.
        private static readonly string userId = Environment.GetEnvironmentVariable("feedly-user-id");
        private static readonly string refreshToken = Environment.GetEnvironmentVariable("feedly-refresh-token");

        private const string FEEDLY_BASE_URL = "https://cloud.feedly.com/v3/";
        private const string KEY_VAULT_BASE_URL = "https://feedly-export-keyvault.vault.azure.net";

        [FunctionName("RefreshFeedlyAuthToken")]
        // ReSharper disable once UnusedParameter.Global
        public static async Task Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"RefreshFeedlyAuthToken function executed at: {DateTime.Now}");
            log.LogInformation($"UserId: {userId}");

            var kv = CreateKeyVaultClient();

            log.LogInformation("Getting current access token contents from key vault");
            var accessToken = await kv.GetSecretAsync(KEY_VAULT_BASE_URL, "feedly-access-token", CancellationToken.None);

            log.LogInformation("Getting refreshed access token from Feedly API");
            var feedlyResponse = await RefreshFeedlyAccessToken(accessToken.Value, log);

            log.LogInformation("Setting the secret in the key vault");
            await kv.SetSecretAsync(KEY_VAULT_BASE_URL, "feedly-access-token", feedlyResponse.access_token);

            log.LogInformation("Successfully updated token in the key vault");
        }

        private static async Task<FeedlyRefreshResponse> RefreshFeedlyAccessToken(string accessToken, ILogger log)
        {
            var client = CreateFeedlyHttpClient(accessToken);

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

            return JsonConvert.DeserializeObject<FeedlyRefreshResponse>(await response.Content.ReadAsStringAsync());

        }
        private static HttpClient CreateFeedlyHttpClient(string accessToken)
        {
            var client = new HttpClient {BaseAddress = new Uri(FEEDLY_BASE_URL)};
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        private static KeyVaultClient CreateKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);

            return new KeyVaultClient(authenticationCallback);
        }
    }

    public class FeedlyRefreshRequest
    {
        private const string FEEDLY_CLIENT_ID = "feedlydev"; // hard-coded for users with Pro accounts

        // ReSharper disable UnusedMember.Global
        // ReSharper disable InconsistentNaming

        // ReSharper disable once MemberCanBePrivate.Global -- used in the http request
        // ReSharper disable once UnusedAutoPropertyAccessor.Global -- used in http request
        public string refresh_token { get; }
        public string client_id =>  FEEDLY_CLIENT_ID;
        public string client_secret => FEEDLY_CLIENT_ID;
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
        public string access_token { get; set; }
        public string plan { get; set; }

    }
}
