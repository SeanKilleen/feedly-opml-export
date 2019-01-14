using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace FeedlyOpmlExport.Functions.Tests
{
    public class OpmlFilteringTests
    {
        [Fact]
        public void FiltersCorrectly()
        {
            var contents = File.ReadAllText("TestOpmlFile.xml");

            var categories = new List<string> {"development", "tech"};
            var result = OpmlFilterer.FilterToCategories(contents, categories);

            result.Should().Contain("text=\"Gizmodo\"");
            result.Should().NotContain("text=\"Docs Blog\"");
        }
    }
}
