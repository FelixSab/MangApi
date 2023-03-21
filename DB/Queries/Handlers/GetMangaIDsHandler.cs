using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DB.Queries.Handlers;

internal class GetMangaIDsHandler : IRequestHandler<GetMangaIDsQuery, IEnumerable<string>>
{
    private readonly MangapiContext _context;

    public GetMangaIDsHandler(MangapiContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<IEnumerable<string>> Handle(GetMangaIDsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Mangas.Select(m => $"{m.ID}").ToListAsync(cancellationToken);
    }
}
