using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace FeedlyOpmlExport.Functions
{
    public static class KeyVaultManager
    {
        private const string KEY_VAULT_BASE_URL = "https://feedly-export-keyvault.vault.azure.net";
        private const string ACCESS_TOKEN_KEY_NAME = "feedly-access-token";

        private static readonly SecretClient theSecretClient = CreateSecretClient();

        public static async Task<string> GetFeedlyAccessToken()
        {
            var accessTokenResponse = await theSecretClient.GetSecretAsync(ACCESS_TOKEN_KEY_NAME);
            var accessToken = accessTokenResponse.Value;
            
            return accessToken.Value;
        }

        private static SecretClient CreateSecretClient()
        {
            var client = new SecretClient(vaultUri: new Uri(KEY_VAULT_BASE_URL), credential: new DefaultAzureCredential());
            return client;
        }

        public static async Task UpdateFeedlyAccessToken(string feedlyResponseAccessToken)
        {
            await theSecretClient.SetSecretAsync(ACCESS_TOKEN_KEY_NAME, feedlyResponseAccessToken);
        }
    }
}