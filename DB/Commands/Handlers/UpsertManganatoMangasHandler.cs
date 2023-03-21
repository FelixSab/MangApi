using DB.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace DB.Commands.Handlers;

internal class UpsertManganatoMangasHandler : IRequestHandler<UpsertManganatoMangasCommand>
{
    private readonly MangapiContext _context;

    public UpsertManganatoMangasHandler(MangapiContext dbContext)
    {
        _context = dbContext;
    }

    public async Task Handle(UpsertManganatoMangasCommand request, CancellationToken cancellationToken)
    {
        foreach (var manga in request.Mangas)
        {
            await UpsertManga(manga);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ----- Local Functions -----

        async Task UpsertManga(Interface.Records.ManganatoManga manga)
        {
            var foundName = await _context.MangaNames
                .Include(nameof(MangaName.Manga))
                .FirstOrDefaultAsync(n => manga.Names.Any(mn => mn == n.Name), cancellationToken: cancellationToken);

            if (foundName is null)
            {
                await InsertManga(manga);
            }
            else
            {
                await UpdateManga(foundName.Manga, manga);
            }
        }

        async Task UpdateManga(Manga dbManga, Interface.Records.ManganatoManga manga)
        {
            var dbManganatoManga = _context.ManganatoMangas.FirstOrDefault(m => m.MangaID == dbManga.ID);
            if (dbManganatoManga is null)
            {
                await InsertManganatoManga(dbManga, manga);
            }
            else
            {
                await UpdateManganatoManga(dbManganatoManga, manga);
            }

            await UpdateChapters(dbManga, manga.Chapters);
        }

        async Task UpdateChapters(Manga manga, IDictionary<string, string> chapters)
        {
            var dbManganatoIds = _context.Chapters
                .Where(c => c.MangaID == manga.ID && c.ManganatoChapter != null)
                .Select(c => c.ManganatoChapter!.ManganatoID).ToImmutableHashSet();

            var chaptersToUpdate = chapters.Where(c => !dbManganatoIds.Contains(c.Key));

            await chaptersToUpdate.SelectSequentially(c => InsertChapter(manga, c));
        }

        async Task InsertChapter(Manga manga, KeyValuePair<string, string> chapter)
        {
            var newChapter = await _context.Chapters.AddAsync(new Chapter
            {
                MangaID = manga.ID,
                Manga = manga,
            }, cancellationToken);

            await _context.ManganatoChapters.AddAsync(new ManganatoChapter
            {
                ManganatoID = chapter.Key,
                Name = chapter.Value,
                ChapterID = newChapter.Entity.ID,
                Chapter = newChapter.Entity
            }, cancellationToken);
        }

        async Task UpdateManganatoManga(ManganatoManga dbManga, Interface.Records.ManganatoManga manga)
        {
            await _context.ManganatoMangas.Where(m => m.ID == dbManga.ID).ExecuteUpdateAsync(u => u
                .SetProperty(m => m.ManganatoID, manga.ManganatoId)
                .SetProperty(m => m.Status, manga.Status)
                .SetProperty(m => m.Rating, manga.Rating)
                .SetProperty(m => m.Votes, manga.Votes)
                .SetProperty(m => m.Views, manga.Views)
                .SetProperty(m => m.Description, manga.Description)
            , cancellationToken);
        }

        async Task InsertManganatoManga(Manga dbManga, Interface.Records.ManganatoManga manga)
        {
            await _context.ManganatoMangas.AddAsync(new ManganatoManga
            {
                MangaID = dbManga.ID,
                Manga = dbManga,
                ManganatoID = manga.ManganatoId,
                Status = manga.Status,
                Rating = manga.Rating,
                Votes = manga.Votes,
                Views = manga.Views,
                Description = manga.Description,
            }, cancellationToken);
        }

        async Task InsertManga(Interface.Records.ManganatoManga manga)
        {
            var newManga = await _context.Mangas.AddAsync(new Manga
            {
                Chapters = new List<Chapter>(),
                Authors = new List<Author>(),
                Genres = new List<Genre>(),
                Names = new List<MangaName>()
            }, cancellationToken);

            await _context.MangaNames.AddRangeAsync(manga.Names.Select(name => new MangaName
            {
                Name = name,
                MangaID = newManga.Entity.ID,
                Manga = newManga.Entity
            }), cancellationToken);

            await InsertManganatoManga(newManga.Entity, manga);

            await manga.Authors.SelectSequentially(UpsertAuthor);
            await manga.Genres.SelectSequentially(UpsertGenre);
            await manga.Chapters.SelectSequentially(c => InsertChapter(newManga.Entity, c));

            // ----- Local Functions -----
            async Task UpsertGenre(string genre)
            {
                if (newManga is null) return;

                var found = await _context.Genres.Include(nameof(Genre.Mangas)).FirstOrDefaultAsync(g => g.Name == genre, cancellationToken: cancellationToken);
                if (found is null)
                {
                    await _context.Genres.AddAsync(new Genre
                    {
                        Name = genre,
                        Mangas = new List<Manga>() { newManga.Entity }
                    }, cancellationToken);
                }
                else
                {
                    found.Mangas.Add(newManga.Entity);
                }
            }

            async Task UpsertAuthor(string author)
            {
                if (newManga is null) return;

                var found = await _context.Authors.Include(nameof(Author.Mangas)).FirstOrDefaultAsync(a => a.Name == author, cancellationToken: cancellationToken);
                if (found is null)
                {
                    await _context.Authors.AddAsync(new Author
                    {
                        Name = author,
                        Mangas = new List<Manga>() { newManga.Entity }
                    }, cancellationToken);
                }
                else
                {
                    found.Mangas.Add(newManga.Entity);
                }
            }
        }
    }
}
