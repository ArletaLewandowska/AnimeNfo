using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Mamut.AnimeNfo.Contract;
using Mamut.AnimeNfo.Services;
using Microsoft.FSharp.Core;
using NUnit.Framework;
using FluentAssertions;

namespace Mamut.AnimeNfo.Tests
{
    [TestFixture]
    public class SitePraserTests
    {
        private static readonly Regex _animeDetailsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline );
        private static readonly Regex _animeUrlsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline);
        private static readonly Regex _nextUrlFromRegex = new Regex(@"<form[^>]*action='/animebyyear.php'[^>]*>.+?</form>", RegexOptions.Singleline);
        
        private static readonly string _animeDetailsPage = new StreamReader(Path.Combine("Files", "AnimeDetailsPage.html")).ReadToEnd();
        private static readonly string _animeUrlsPage = new StreamReader(Path.Combine("Files", "AnimeUrlsPage.html")).ReadToEnd();

        private static readonly string _studioValue = @"<a href=""animestudio,524,nggzzl,seven.html"">
		Seven</a><br />";

        private static readonly string _nextUrlElement =
            @"<a href='/animebyyear.php?year=1989&amp;pagenumber=2&amp;perpage=30&amp;action=go'>Next</a>";

        private static readonly Dictionary<string, string> _expectedMatchedGroups = new Dictionary<string, string>
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
                {"Studio", _studioValue},
                {"US Distribution", ""},
                {"User Rating", "N/A"},
                {"Updated", "Tue, 25 Dec 2012 16:51:07 -0500"}
            };

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

            _expectedMatchedGroups.Count.Should().Be(matchCollection.Count);
            _expectedMatchedGroups.Keys.All(k =>
                {
                    var single = matchCollection.Cast<Match>().Single(m => m.Groups["key"].Value == k);
                    return _expectedMatchedGroups[k] == single.Groups["value"].Value;
                })
                .Should()
                .BeTrue();

        }

        [Test]
        public void SiteParserShouldProcessAnime()
        {
            Anime anime = SitePraser.animeFromPage(_animeDetailsPage);

            anime.Title.Should().Be(_expectedMatchedGroups["Title"]);
            anime.JapaneseTitle.Should().Be(_expectedMatchedGroups["Japanese Title"]);
            anime.OfficialSite.Should().Be(_expectedMatchedGroups["Official Site"]);
            anime.Category.Value.ToString().Should().Be(_expectedMatchedGroups["Category"]);
            anime.TotalEpisodes.Should().Be(_expectedMatchedGroups["Total Episodes"]);
            anime.Genres.Count().Should().Be(0);
            anime.YearPublished.ToString().Should().Be(_expectedMatchedGroups["Year Published"]);

            if (anime.ReleaseDate != FSharpOption<DateTime>.None)
                anime.ReleaseDate.Value.ToString("yyyy-MM-dd").Should().Be(_expectedMatchedGroups["Release Date"]);

            anime.Broadcaster.Should().Be(_expectedMatchedGroups["Broadcaster"]);
            anime.Studio.Should().Be("Seven");
            anime.USDistribution.Should().Be(_expectedMatchedGroups["US Distribution"]);

            if (_expectedMatchedGroups["User Rating"] == "N/A")
                anime.UserRating.Should().Be(FSharpOption<Rating>.None);
            else
            {
                Assert.Fail("Finish assertion");
            }

            anime.Updated.ToString("r").Should().StartWith(_expectedMatchedGroups["Updated"].Substring(0, 17));
        }

        [Test]
        public void ShouldMatchAnimeUrlsTable()
        {
            var match = _animeUrlsTableRegex.Match(_animeUrlsPage);

            match.Success.Should().BeTrue();
            
            match.Value.Should().Contain("animetitle,2136,yngeyf,wild_animals_i_.html");
            match.Value.Should().Contain("animetitle,2142,fxpool,wrestler_gundan.html");
            match.Value.Should().Contain("animetitle,3363,twxdxg,yajikita_gakuen.html");
            match.Value.Should().Contain("animetitle,2770,hiprvl,yankee_reppu_ta.html");
            match.Value.Should().Contain("animetitle,520,jjjodz,yawara__a_fashi.html");

        }

        [Test]
        public void ShouldMatchAnimeUrls()
        {
            var animeTable = _animeUrlsTableRegex.Match(_animeUrlsPage).Value;
            var urlsRegex = new Regex(@"href=""(?<url>.+?)""");

            var matches = urlsRegex.Matches(animeTable)
                .Cast<Match>()
                .Select(m => m.Groups["url"].Value)
                .ToList();

            matches.Should().Contain("animetitle,2136,yngeyf,wild_animals_i_.html");
            matches.Should().Contain("animetitle,2142,fxpool,wrestler_gundan.html");
            matches.Should().Contain("animetitle,3363,twxdxg,yajikita_gakuen.html");
            matches.Should().Contain("animetitle,2770,hiprvl,yankee_reppu_ta.html");
            matches.Should().Contain("animetitle,520,jjjodz,yawara__a_fashi.html");
        }

        [Test]
        public void SiteParserShouldGetAnimeUrls()
        {
            var urlsFromPage = SitePraser.urlsFromPage(_animeUrlsPage).ToList();

            urlsFromPage.Should().Contain("http://www.animenfo.com/animetitle,2136,yngeyf,wild_animals_i_.html");
            urlsFromPage.Should().Contain("http://www.animenfo.com/animetitle,2142,fxpool,wrestler_gundan.html");
            urlsFromPage.Should().Contain("http://www.animenfo.com/animetitle,3363,twxdxg,yajikita_gakuen.html");
            urlsFromPage.Should().Contain("http://www.animenfo.com/animetitle,2770,hiprvl,yankee_reppu_ta.html");
            urlsFromPage.Should().Contain("http://www.animenfo.com/animetitle,520,jjjodz,yawara__a_fashi.html");
        }

        [Test]
        public void ShouldMatchNextUrlForm()
        {
            var nextUrlFormMatch = _nextUrlFromRegex.Match(_animeUrlsPage);
            
            nextUrlFormMatch.Success.Should().BeTrue();
            nextUrlFormMatch.Value.Should().Contain("Prev");
        }

        [Test]
        public void ShouldMatchNextUrl()
        {
            var nextUrlForm = _nextUrlFromRegex.Match(_animeUrlsPage).Value;
            var _nextUrlRegex = new Regex(@"<a[^>]*?href='(?<value>[^']+)'[^>]*>Next</a>", RegexOptions.Singleline);
            _nextUrlRegex.Match(nextUrlForm).Success.Should().BeFalse();
            var match1 = _nextUrlRegex
                .Match("<a href='/animebyyear.php?year=2011&amp;pagenumber=1&amp;perpage=30&amp;action=go'>Prev</a> | <a href='/animebyyear.php?year=2011&amp;pagenumber=3&amp;perpage=30&amp;action=go'>Next</a>");
            match1.Success.Should().BeTrue();
            match1.Groups["value"].Value.Should().Be("/animebyyear.php?year=2011&amp;pagenumber=3&amp;perpage=30&amp;action=go");

            var properMatch = _nextUrlRegex.Match(_nextUrlElement);
            properMatch.Groups["value"].Value.Should().Be("/animebyyear.php?year=1989&amp;pagenumber=2&amp;perpage=30&amp;action=go");
        }

        [Test]
        public void SiteParserShouldGetNextUrl()
        {
            var nextPage = SitePraser.nextUrl(_animeUrlsPage);
            nextPage.Should().Be(FSharpOption<string>.None);
        }

        [Test]
        public void SiteParserShoulGetValidYearUrls()
        {
            var cancellationToken = new CancellationToken();
            var animeByYearUrls = Microsoft.FSharp.Control.FSharpAsync.RunSynchronously(
                SiteService.animeByYearUrls("http://animenfo.com/animebyyear.php?year=2007&perpage=30"),
                new FSharpOption<int>(int.MaxValue),
                new FSharpOption<CancellationToken>(cancellationToken));

            animeByYearUrls.Should().NotBeEmpty();
        }

        [Test]
        public void ShouldParseDate()
        {
            DateTime result;
            DateTime.TryParseExact("2013-02-16", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result).Should().BeTrue();
            result.Should().Be(new DateTime(2013, 2, 16));
        }

        [Test]
        public void ShouldMatchUserRating()
        {
            var ratingText1 = @"9.5/10.0 (229 reviews)&nbsp;&nbsp;(<a href='/statistic/top.php?l=anime'>top 200</a>: # 1)&nbsp;&nbsp;(<a href="";""JavaScript: OpenWindow('animereviewdetailstatistic','/anime/review/stats.php?id=315','500','500')"">Statistic</a>)";
            var regex = new Regex(@"(?<rating>\d\.\d).+?\((?<reviewCount>\d+)");
            var match1 = regex.Match(ratingText1);
            match1.Success.Should().BeTrue();
            match1.Groups["rating"].Value.Should().Be("9.5");
            match1.Groups["reviewCount"].Value.Should().Be("229");

            var ratingText2 = @"9.2/10.0 (1 review)&nbsp;&nbsp;(<a href=""JavaScript: OpenWindow('animereviewdetailstatistic','/anime/review/stats.php?id=4933','500','500')"">Statistic</a>)";
            var match2 = regex.Match(ratingText2);
            match2.Success.Should().BeTrue();
            match2.Groups["rating"].Value.Should().Be("9.2");
            match2.Groups["reviewCount"].Value.Should().Be("1");
        }

        [Test]
        public void ShouldExtractGenres()
        {
            const string genresCell = @"<script type='text/javascript'>
function sgn(id){if(id==-1)self.status='';else self.status=gn_array[id*2+1];}
</script><a href='animebygenre.php?genre=1'>Action</a>, <a href='animebygenre.php?genre=11'>Drama</a>, <a href='animebygenre.php?genre=15'>Historical Settings</a>, <a href='animebygenre.php?genre=7'>Romance</a>
<script type='text/javascript'>
var gn_array=new Array(1,""Action"",11,""Drama"",15,""Historical Settings"",7,""Romance"");
</script>";
            var genresRegex = new Regex("<a href.+?>(?<genre>.+?)</a>");

            var expectedGenres = new[] {"Action", "Drama", "Historical Settings", "Romance"};
            var matchCollection = genresRegex.Matches(genresCell);

            matchCollection.Should().HaveCount(expectedGenres.Length);
            matchCollection
                .Cast<Match>().Select(m => m.Groups["genre"].Value)
                .Zip(expectedGenres, (g1, g2) => g1 == g2)
                .All(b => b)
                .Should().BeTrue();
        }

        [Test]
        public void ShouldParseStuff()
        {
            var officialSite = @"<a href=""animestudio,541,ueziiw,.hack_conglomer.html"">
		.hack Conglomerate</a><br />";
            var regex = new Regex(@"href=""(?<url>.+?)""[^>]*>\s*(?<text>.+?)<", RegexOptions.Singleline);

            var match = regex.Match(officialSite);
            match.Success.Should().BeTrue();
            match.Groups["text"].Value.Should().Be(".hack Conglomerate");
        }


    }
}
