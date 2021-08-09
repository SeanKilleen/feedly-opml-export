using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FeedlyOpmlExport.Functions.Utilities;
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

            var categories = new List<string> { "development", "tech", "development - discover.net", "devops", "agile" };
            var filteredDoc = OpmlFilterer.FilterToCategories(opmlXml, categories);

            log.LogInformation("After filtering: ");
            log.LogInformation(filteredDoc);

            var filterAndLabeledDoc = OpmlLabeler.LabelOpmlFile(filteredDoc);
            log.LogInformation("After labeling: ");
            log.LogInformation(filterAndLabeledDoc);

            log.LogInformation("Saving to the blob");
            var thing = Encoding.Default.GetBytes(filterAndLabeledDoc);
            await blobOutput.WriteAsync(thing, 0, thing.Length);

            log.LogInformation("Finished!");
        }
    }
}
