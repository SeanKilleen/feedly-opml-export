using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FeedlyOpmlExport.Functions
{
    public static class ExtractFeedlyOPML
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myTimer">Runs at 5am every day. 5am is a nice time of day.</param>
        /// <param name="log"></param>
        [FunctionName("ExtractFeedlyOPML")]
        public static async Task Run([TimerTrigger("0 0 5 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var accessToken = await KeyVaultManager.GetFeedlyAccessToken();
            var opmlXml = await FeedlyManager.GetOpmlContents(accessToken);

            //TODO Remove
            log.LogDebug("Full XML:");
            log.LogDebug(opmlXml);

            var categories = new List<string> { "development", "tech" };
            var output = OpmlFilterer.FilterToCategories(opmlXml, categories);

            log.LogInformation("After filtering: ");
            log.LogInformation(output);
        }
    }

    public static class OpmlFilterer
    {
        public static string FilterToCategories(string opmlXml, List<string> categories)
        {
            var opmlDoc = XElement.Parse(opmlXml);

            opmlDoc.Descendants("body")
                .DescendantNodes()
                .Where(x =>
                {
                    var containerized = (XElement)x;
                    return !categories.Contains(containerized.Attribute("title")?.Value.ToLowerInvariant());
                })
                .Remove();

            return opmlDoc.ToString();
        }
    }
}
