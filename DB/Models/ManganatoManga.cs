namespace DB.Models;

internal record ManganatoManga : BaseModel
{
    public required string ManganatoID { get; init; }
    public required string Status { get; init; }
    public required double Rating { get; init; }
    public required int Votes { get; init; }
    public required int Views { get; init; }
    public required string Description { get; init; }
    public required int MangaID { get; init; }


    public required Manga Manga { get; init; }
}
