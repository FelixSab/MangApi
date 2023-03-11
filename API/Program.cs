using API.Cron;
using API.Cron.Jobs;
using Crawler;
using DB;
using Interface;
using Microsoft.AspNetCore.Mvc;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddQuartzConfigured();
builder.Services.AddMangapiContext();
builder.Services.AddManganatoCrawler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/crawl-manganato", async ([FromServices] IManganatoCrawler crawler, [FromServices] IDbContext context, [FromQuery] int? pageEnd, [FromQuery] int? pageStart) =>
{
    if (pageEnd is null)
    {
        var mangas = await crawler.CrawlRecentlyUpdatedMangas();
        await context.UpsertManganatoMangas(mangas);
    }
    else
    {
        var mangas = await crawler.CrawlMangaPages(pageStart ?? 1, pageEnd ?? 1);
        await context.UpsertManganatoMangas(mangas);
    }
});

app.MapGet("/get-mangas", ([FromServices] IDbContext context) =>
{
    return context.GetMangaIDs();
}).WithOpenApi();

app.Run();
