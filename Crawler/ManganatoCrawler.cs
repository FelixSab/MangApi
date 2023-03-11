using HtmlAgilityPack;
using Interface;
using Interface.Records;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace Crawler;

internal partial class ManganatoCrawler : IManganatoCrawler
{
    private const string _baseManganatoUrl = "https://manganato.com/manga-";
    private const string _alternativeManganatoUrl = "https://chapmanganato.com/manga-";
    private readonly HtmlWeb _crawler = new();

    public async Task<IEnumerable<ManganatoManga>> CrawlRecentlyUpdatedMangas()
    {
        string url = "https://manganato.com/";

        // load the web page and parse it with HtmlAgilityPack
        var document = await _crawler.LoadFromWebAsync(url);

        var recentlyUpdatedMangas = document.DocumentNode.SelectSingleNodeByClass("panel-content-homepage");

        var links = GetLinks(_baseManganatoUrl).Union(GetLinks(_alternativeManganatoUrl))
            .Where(l => MangaLinkRegex().IsMatch(l));

        if (links is null) return Enumerable.Empty<ManganatoManga>();
        
        var li = new List<ManganatoManga>();
        foreach (var l in links)
        {
            li.Add(await CrawlManga(l));
        }

        return li;

        // ----- Local Functions -----
        IEnumerable<string> GetLinks(string url)
        {
            return recentlyUpdatedMangas!.SelectNodes($".//a[contains(@href, '{url}')]")
            .Select(node => node.Attributes["href"].Value)
            .Distinct();
        }
    }

    public async Task<IEnumerable<ManganatoManga>> CrawlMangaPages(int pageStart = 1, int pageEnd = 1)
    {
        string url = "https://manganato.com/genre-all/";
        var li = new List<ManganatoManga>();

        for (int currentPage = pageStart; currentPage <= pageEnd; currentPage++)
        {
            // load the web page and parse it with HtmlAgilityPack
            var document = await _crawler.LoadFromWebAsync($"{url}{currentPage}");

            var mangas = document.DocumentNode.SelectSingleNodeByClass("panel-content-genres");

            var links = GetLinks(mangas, _baseManganatoUrl).Union(GetLinks(mangas, _alternativeManganatoUrl))
                .Where(l => MangaLinkRegex().IsMatch(l));

            if (links is null) continue;

            foreach (var l in links)
            {
                li.Add(await CrawlManga(l));
            }
        }

        return li;

        // ----- Local Functions -----
        IEnumerable<string> GetLinks(HtmlNode mangas, string url)
        {
            return mangas.SelectNodes($".//a[contains(@href, '{url}')]")
            .Select(node => node.Attributes["href"].Value)
            .Distinct();
        }
    }

    public async Task<ManganatoManga> CrawlManga(string mangaUrl)
    {
        var document = await _crawler.LoadFromWebAsync($"{mangaUrl}");
        var container = document.DocumentNode.SelectSingleNodeByClass("story-info-right");
        var values = container.SelectNodesByClass("table-value");

        var name = document.DocumentNode.SelectSingleNode(".//h1").InnerText.Trim();

        var tuple = HandleValues(name, values);

        var names = tuple.Item1;
        var mangaAuthor = tuple.Item2;
        var mangaStatus = tuple.Item3;
        var mangaGenres = tuple.Item4;

        _ = double.TryParse(container.SelectSingleNode(".//*[@property='v:average']").InnerText.Trim(), CultureInfo.InvariantCulture, out var rating);
        _ = int.TryParse(container.SelectSingleNode(".//*[@property='v:votes']").InnerText.Trim(), out var votes);

        var moreValues = container.SelectNodesByClass("stre-value");
        _ = DateTime.TryParseExact(moreValues[0].InnerText, "MMM dd,yyyy - hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updatedAt);
        var views = moreValues[1].InnerText.ParseShortifiedNumber();

        var description = document.DocumentNode.SelectSingleNodeByClass("panel-story-info-description").ChildNodes[2].InnerText.Trim();

        var chapters = TryGetChapters(document.DocumentNode);

        return new ManganatoManga
        {
            ManganatoId = mangaUrl,
            Names = names,
            Authors = mangaAuthor,
            Status = mangaStatus,
            Genres = mangaGenres,
            Votes = votes,
            Rating = rating,
            Views = views,
            Description = description,
            Chapters = chapters,
            UpdatedAt = updatedAt,
        };
    }

    private static IDictionary<string, string> TryGetChapters(HtmlNode node)
    {
        try
        {
            var container = node.SelectSingleNodeByClass("row-content-chapter");
            var chapters = container.SelectNodesByClass("chapter-name");

            return new Dictionary<string, string>(chapters.Reverse().Select(c =>
            {
                int lastIndex = c.Attributes["href"].Value.LastIndexOf('/');
                string chapterId = c.Attributes["href"].Value[(lastIndex + 1)..];

                return KeyValuePair.Create(chapterId, c.InnerText.Trim());
            }));
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private static (IEnumerable<string>, IEnumerable<string>, string, IEnumerable<string>) HandleValues(string mangaName, HtmlNodeCollection values)
    {
        var baseIndex = 0;

        if (values.Count == 3)
        {
            baseIndex = -1;
        }

        var mangaAuthors = values[baseIndex + 1].InnerText.Split(";").Select(author => author.Trim());
        var mangaStatus = values[baseIndex + 2].InnerText.Trim();
        var mangaGenres = values[baseIndex + 3].ChildNodes
            .Select(n => n.InnerText.Trim())
            .Where(genre => genre != "-" && genre != string.Empty);


        if (values.Count == 4)
        {
            var names = new List<string>(values[0].FirstChild.InnerText.Split(";").Select(name => name.Trim()))
            {
                mangaName
            };

            return (names, mangaAuthors, mangaStatus, mangaGenres);
        }

        return (new[] { mangaName }, mangaAuthors, mangaStatus, mangaGenres);
    }

    [GeneratedRegex("^https:\\/\\/(chap)?manganato.com\\/manga-([a-z0-9]{8})$")]
    private static partial Regex MangaLinkRegex();
}
