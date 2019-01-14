using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FeedlyOpmlExport.Functions
{
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