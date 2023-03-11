namespace DB.Models;

internal record Author : BaseModel
{
    public required string Name { get; init; }

    public required ICollection<Manga> Mangas { get; init; }
}
