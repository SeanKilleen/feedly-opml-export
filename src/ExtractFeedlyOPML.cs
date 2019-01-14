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
            var filteredDoc = OpmlFilterer.FilterToCategories(opmlXml, categories);

            log.LogInformation("After filtering: ");
            log.LogInformation(filteredDoc);

            var filterAndLabeledDoc = OpmlLabeler.LabelOpmlFile(filteredDoc);
            log.LogInformation("After labeling: ");
            log.LogInformation(filterAndLabeledDoc);

            log.LogInformation("Saving to the blob");
            var thing = Encoding.Default.GetBytes(filterAndLabeledDoc);
            await blobOutput.WriteAsync(thing, 0, thing.Length);

            log.LogInformation("Done!");
        }
    }

    public static class OpmlLabeler
    {
        public static string LabelOpmlFile(string opmlXml)
        {
            var opmlDoc = XElement.Parse(opmlXml);

            var firstLevelChildren = opmlDoc.DescendantNodes().Where(x => x.Parent.Name == "body");

            foreach (var child in firstLevelChildren)
            {
                var containerized = (XElement)child;

                var titleAttribute = containerized.Attribute(XName.Get("title"));
                titleAttribute.SetValue(titleAttribute.Value + " - via Sean Killeen");

                var textAttribute = containerized.Attribute(XName.Get("text"));
                textAttribute.SetValue(titleAttribute.Value + " - via Sean Killeen");
            }

            return opmlDoc.ToString();
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
