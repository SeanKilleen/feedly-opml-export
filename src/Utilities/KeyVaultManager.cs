using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace FeedlyOpmlExport.Functions
{
    public static class KeyVaultManager
    {
        private const string KEY_VAULT_BASE_URL = "https://feedly-export-keyvault.vault.azure.net";
        private const string ACCESS_TOKEN_KEY_NAME = "feedly-access-token";

        private static KeyVaultClient theClient = CreateKeyVaultClient();

        public static async Task<string> GetFeedlyAccessToken()
        {
            var accessToken = await theClient.GetSecretAsync(KEY_VAULT_BASE_URL, ACCESS_TOKEN_KEY_NAME, CancellationToken.None);
            return accessToken.Value;
        }
        private static KeyVaultClient CreateKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);

            return new KeyVaultClient(authenticationCallback);
        }

        public static async Task UpdateFeedlyAccessToken(string feedlyResponseAccessToken)
        {
            await theClient.SetSecretAsync(KEY_VAULT_BASE_URL, ACCESS_TOKEN_KEY_NAME, feedlyResponseAccessToken);
        }
    }
}