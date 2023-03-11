using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DB.Models;

[PrimaryKey(nameof(ID))]
internal abstract record BaseModel
{
    public int ID { get; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
