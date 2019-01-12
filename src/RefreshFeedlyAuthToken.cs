using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FeedlyOpmlExport.Functions
{
    public static class RefreshFeedlyAuthToken
    {
        [FunctionName("RefreshFeedlyAuthToken")]
        public static void Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
