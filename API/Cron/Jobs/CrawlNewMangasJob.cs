using API.Cron.Interfaces;
using Crawler;
using Interface;
using Quartz;

namespace API.Cron.Jobs;

internal class CrawlNewMangasJob : ICronJob
{
    private readonly IServiceProvider _serviceProvider;

    public static string CronSchedule => "0 0/30 * * * ?";

    public static string Name => nameof(CrawlNewMangasJob);

    public CrawlNewMangasJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var crawler = _serviceProvider.GetService<IManganatoCrawler>()!;
        var dbContext = _serviceProvider.GetService<IDbContext>()!;

        var mangas = await crawler.CrawlRecentlyUpdatedMangas();
        await dbContext.UpsertManganatoMangas(mangas);
    }
}
