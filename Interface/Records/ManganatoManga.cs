using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Records;

public record ManganatoManga
{
    public required string ManganatoId { get; init; }
    public required IEnumerable<string> Names { get; init; }
    public required IEnumerable<string> Authors { get; init; }
    public required string Status { get; init; }
    public required IEnumerable<string> Genres { get; init; }
    public required double Rating { get; init; }
    public required int Votes { get; init; }
    public required int Views { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string Description { get; init; }
    public required IDictionary<string, string> Chapters { get; init; }
}
