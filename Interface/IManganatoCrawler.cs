using Interface.Records;

namespace Interface;

public interface IManganatoCrawler
{
    Task<ManganatoManga> CrawlManga(string manganatoId);
    Task<IEnumerable<ManganatoManga>> CrawlRecentlyUpdatedMangas();
    Task<IEnumerable<ManganatoManga>> CrawlMangaPages(int pageStart = 1, int pageEnd = 1);
}
