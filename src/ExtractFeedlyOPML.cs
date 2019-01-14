using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public static async Task Run(
            [TimerTrigger("0 0 5 * * *")]TimerInfo myTimer
            , ILogger log
            , [Blob("opml-file/SeanKilleenBlogs.opml", FileAccess.Write)] Stream blobOutput)
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

            log.LogInformation("Saving to the blob");
            var thing = Encoding.Default.GetBytes(output);
            await blobOutput.WriteAsync(thing, 0, thing.Length);

            log.LogInformation("Done!");
        }
    }

    public static class OpmlFilterer
    {
        public static string FilterToCategories(string opmlXml, List<string> categories)
        {
            var opmlDoc = XElement.Parse(opmlXml);

            opmlDoc.Descendants("body").DescendantNodes()
                .Where(x =>
                {
                    var containerized = (XElement)x;

                    return x.Parent.Name == "body"
                           && !categories.Contains(containerized.Attribute(XName.Get("title"))?.Value.ToLowerInvariant());
                })
                .Remove();

            return opmlDoc.ToString();
        }
    }
}
