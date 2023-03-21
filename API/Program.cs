using API.Cron;
using Crawler;
using DB;
using Interface;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddQuartzConfigured();
builder.Services.AddMangapiContext();
builder.Services.AddManganatoCrawler();
builder.Services.AddMediator();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/crawl-manganato", async ([FromServices] IManganatoCrawler crawler, [FromServices] IMediator mediator, [FromQuery] int? pageEnd, [FromQuery] int? pageStart) =>
{
    if (pageEnd is null)
    {
        var mangas = await crawler.CrawlRecentlyUpdatedMangas();
        await mediator.UpsertManganatoMangas(mangas);
    }
    else
    {
        var mangas = await crawler.CrawlMangaPages(pageStart ?? 1, pageEnd ?? 1);
        await mediator.UpsertManganatoMangas(mangas);
    }
});

app.MapGet("/get-mangas", ([FromServices] IMediator mediator) =>
{
    return mediator.GetMangaIDs();
}).WithOpenApi();

app.Run();
