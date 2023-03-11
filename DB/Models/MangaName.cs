namespace DB.Models;

internal record MangaName : BaseModel
{
    public required string Name { get; init; }
    public required int MangaID { get; init; }

    public required Manga Manga { get; init; }
}
