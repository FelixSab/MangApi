using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace DB;

internal class MangapiContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Manga> Mangas { get; set; }
    public DbSet<MangaName> MangaNames { get; set; }
    public DbSet<ManganatoChapter> ManganatoChapters { get; set; }
    public DbSet<ManganatoManga> ManganatoMangas { get; set; }

    public MangapiContext() { }

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
}
