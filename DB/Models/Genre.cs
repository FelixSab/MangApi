namespace DB.Models;

internal record Genre : BaseModel
{
    public required string Name { get; init; }


    public required ICollection<Manga> Mangas { get; init; }
}
