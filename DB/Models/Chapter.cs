namespace DB.Models;

internal record Chapter : BaseModel
{
    public required int MangaID { get; init; }
    public string Name { get; init; } = "";

    public required Manga Manga { get; init; }
    public ManganatoChapter? ManganatoChapter { get; init; }
}
