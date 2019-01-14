using System.Linq;
using System.Xml.Linq;

namespace FeedlyOpmlExport.Functions
{
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
                textAttribute.SetValue(textAttribute.Value + " - via Sean Killeen");
            }

            return opmlDoc.ToString();
        }
    }
}