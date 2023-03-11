using DB.Models;
using Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB;

internal class MangapiContext : DbContext, IDbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Manga> Mangas { get; set; }
    public DbSet<MangaName> MangaNames { get; set; }
    public DbSet<ManganatoChapter> ManganatoChapters { get; set; }
    public DbSet<ManganatoManga> ManganatoMangas { get; set; }

    public MangapiContext()
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=mangapi;User Id=postgres;Password=ret6ewt9w8ethj0wq3;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Chapter>()
            .HasOne(c => c.ManganatoChapter)
            .WithOne(c => c.Chapter)
            .HasForeignKey<ManganatoChapter>(c => c.ChapterID);
    }

    public async Task UpsertManganatoMangas(IEnumerable<Interface.Records.ManganatoManga> mangas)
    {
        foreach (var manga in mangas)
        {
            await UpsertManga(manga);
            await SaveChangesAsync();
        }

        // ----- Local Functions -----

        async Task UpsertManga(Interface.Records.ManganatoManga manga)
        {
            var foundName = await MangaNames
                .Include(nameof(MangaName.Manga))
                .FirstOrDefaultAsync(n => manga.Names.Any(mn => mn == n.Name));


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
            var dbManganatoManga = ManganatoMangas.FirstOrDefault(m => m.MangaID == dbManga.ID);
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
            var dbManganatoIds = Chapters
                .Where(c => c.MangaID == manga.ID && c.ManganatoChapter != null)
                .Select(c => c.ManganatoChapter!.ManganatoID);

            var chaptersToUpdate = chapters.Where(c => !dbManganatoIds.Contains(c.Key));

            await chaptersToUpdate.SelectSequentially(c => InsertChapter(manga, c));
        }

        async Task InsertChapter(Manga manga, KeyValuePair<string, string> chapter)
        {
            var newChapter = await Chapters.AddAsync(new Chapter
            {
                MangaID = manga.ID,
                Manga = manga,
            });

            await ManganatoChapters.AddAsync(new ManganatoChapter
            {
                ManganatoID = chapter.Key,
                Name = chapter.Value,
                ChapterID = newChapter.Entity.ID,
                Chapter = newChapter.Entity
            });
        }

        async Task UpdateManganatoManga(ManganatoManga dbManga, Interface.Records.ManganatoManga manga)
        {
            await ManganatoMangas.Where(m => m.ID == dbManga.ID).ExecuteUpdateAsync(u => u
                .SetProperty(m => m.ManganatoID, manga.ManganatoId)
                .SetProperty(m => m.Status, manga.Status)
                .SetProperty(m => m.Rating, manga.Rating)
                .SetProperty(m => m.Votes, manga.Votes)
                .SetProperty(m => m.Views, manga.Views)
                .SetProperty(m => m.Description, manga.Description)
            );
        }

        async Task InsertManganatoManga(Manga dbManga, Interface.Records.ManganatoManga manga)
        {
            await ManganatoMangas.AddAsync(new ManganatoManga
            {
                MangaID = dbManga.ID,
                Manga = dbManga,
                ManganatoID = manga.ManganatoId,
                Status = manga.Status,
                Rating = manga.Rating,
                Votes = manga.Votes,
                Views = manga.Views,
                Description = manga.Description,
            });
        }

        async Task InsertManga(Interface.Records.ManganatoManga manga)
        {
            var newManga = await Mangas.AddAsync(new Manga
            {
                Chapters = new List<Chapter>(),
                Authors = new List<Author>(),
                Genres = new List<Genre>(),
                Names = new List<MangaName>()
            });

            await MangaNames.AddRangeAsync(manga.Names.Select(name => new MangaName
            {
                Name = name,
                MangaID = newManga.Entity.ID,
                Manga = newManga.Entity
            }));

            await InsertManganatoManga(newManga.Entity, manga);

            await manga.Authors.SelectSequentially(UpsertAuthor);
            await manga.Genres.SelectSequentially(UpsertGenre);
            await manga.Chapters.SelectSequentially(c => InsertChapter(newManga.Entity, c));

            // ----- Local Functions -----
            async Task UpsertGenre(string genre)
            {
                if (newManga is null) return;

                var found = await Genres.Include(nameof(Genre.Mangas)).FirstOrDefaultAsync(g => g.Name == genre);
                if (found is null)
                {
                    await Genres.AddAsync(new Genre
                    {
                        Name = genre,
                        Mangas = new List<Manga>() { newManga.Entity }
                    });
                }
                else
                {
                    found.Mangas.Add(newManga.Entity);
                }
            }

            async Task UpsertAuthor(string author)
            {
                if (newManga is null) return;

                var found = await Authors.Include(nameof(Author.Mangas)).FirstOrDefaultAsync(a => a.Name == author);
                if (found is null)
                {
                    await Authors.AddAsync(new Author
                    {
                        Name = author,
                        Mangas = new List<Manga>() { newManga.Entity }
                    });
                }
                else
                {
                    found.Mangas.Add(newManga.Entity);
                }
            }
        }
    }

    public IEnumerable<string> GetMangaIDs()
    {
        return Mangas.Select(m => $"{m.ID}");
    }
}
