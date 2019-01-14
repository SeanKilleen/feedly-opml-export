using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeedlyOpmlExport.Functions
{
    public static class FeedlyManager
    {
        private const string FEEDLY_BASE_URL = "https://cloud.feedly.com/v3/";

        public static async Task<FeedlyRefreshResponse> RefreshFeedlyAccessToken(string accessToken, ILogger log, string refreshToken)
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

            return JsonConvert.DeserializeObject<FeedlyRefreshResponse>(await response.Content.ReadAsStringAsync());

        }

        public static async Task<string> GetOpmlContents(string accessToken)
        {
            var client = CreateFeedlyHttpClient(accessToken);

            var response = await client.GetAsync("opml");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private static HttpClient CreateFeedlyHttpClient(string accessToken)
        {
            var client = new HttpClient { BaseAddress = new Uri(FEEDLY_BASE_URL) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

    }
}