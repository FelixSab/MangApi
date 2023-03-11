namespace DB.Models;

internal record Manga : BaseModel
{
    public ManganatoManga? ManganatoManga { get; init; }
    public required ICollection<Chapter> Chapters { get; init; }
    public required ICollection<MangaName> Names { get; init; }
    public required ICollection<Author> Authors { get; init; }
    public required ICollection<Genre> Genres { get; init; }
}
