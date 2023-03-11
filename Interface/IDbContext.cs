using Interface.Records;

namespace Interface;

public interface IDbContext
{
    public Task UpsertManganatoMangas(IEnumerable<ManganatoManga> mangas);
    public IEnumerable<string> GetMangaIDs();
}
