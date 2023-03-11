using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

internal record ManganatoChapter : BaseModel
{
    public required string ManganatoID { get; init; }
    public required int ChapterID { get; init; }
    public required string Name { get; init; }

    public required Chapter Chapter { get; init; }
}
