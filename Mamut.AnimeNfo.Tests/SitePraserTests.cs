using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using FluentAssertions;

namespace Mamut.AnimeNfo.Tests
{
    [TestFixture]
    public class SitePraserTests
    {
        private static readonly Regex _animeDetailsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline );
        private static readonly string _animeDetailsPage = new StreamReader(Path.Combine("Files", "AnimeDetailsPage.html")).ReadToEnd();

        [Test]
        public void ShouldMatchAnimeTable()
        {
            var match = _animeDetailsTableRegex.Match(_animeDetailsPage);

            match.Success.Should().BeTrue();
            match.Value.Should().Contain(@"<img class=""float"" src=""image/anime_5246.jpg"" alt=""Ai Mai Mi"" />");
            match.Value.Should().Contain("Title");
            match.Value.Should().Contain("Japanese Title");
            match.Value.Should().Contain("Official Site");
            match.Value.Should().Contain("Category");
            match.Value.Should().Contain("Total Episodes");
            match.Value.Should().Contain("Genres");
            match.Value.Should().Contain("Year Published");
            match.Value.Should().Contain("Release Date");
            match.Value.Should().Contain("Broadcaster");
            match.Value.Should().Contain("Studio");
            match.Value.Should().Contain("US Distribution");
            match.Value.Should().Contain("User Rating");
            match.Value.Should().Contain("Updated"); 
        }

        [Test]
        public void ShouldMatchImageSource()
        {
            var animeDetailsTable = _animeDetailsTableRegex.Match(_animeDetailsPage).Value;
            var imageRegex = new Regex("src=\"(?<link>.*?)\"");
            imageRegex.Match(animeDetailsTable).Groups["link"].Value.Should().Be("image/anime_5246.jpg");
        }

        [Test]
        public void ShouldMatchAnimeTextualData()
        {
            var animeDetailsTable = _animeDetailsTableRegex.Match(_animeDetailsPage).Value;
            var textualDataRegex = new Regex("<td.*?<b>(?<key>.*?)</b>.*?<td.*?>((<a.*?>(?<value>.*?)</a>)|(?<value>.*?))</td>", RegexOptions.Singleline);
            var matchCollection = textualDataRegex.Matches(animeDetailsTable);
            var studioValue =
@"<a href=""animestudio,524,nggzzl,seven.html"">
		Seven</a><br />";
            var expectedMatchedGroups = new Dictionary<string, string>
                {
                    {"Title", "Ai Mai Mi"},
                    {"Japanese Title", "あいまいみー"},
                    {"Official Site", "http://www.takeshobo.co.jp/sp/tv_aimaimi/"},
                    {"Category", "TV"},
                    {"Total Episodes", "-"},
                    {"Genres", "-"},
                    {"Year Published", "2013"},
                    {"Release Date", "2013-01-03 &sim;"},
                    {"Broadcaster", "-"},
                    {"Studio", studioValue},
                    {"US Distribution", ""},
                    {"User Rating", "N/A"},
                    {"Updated", "Tue, 25 Dec 2012 16:51:07 -0500"}
                };

            expectedMatchedGroups.Count.Should().Be(matchCollection.Count);
            expectedMatchedGroups.Keys.All(k =>
                {
                    var single = matchCollection.Cast<Match>().Single(m => m.Groups["key"].Value == k);
                    return expectedMatchedGroups[k] == single.Groups["value"].Value;
                })
                .Should()
                .BeTrue();

        }
    }
}
