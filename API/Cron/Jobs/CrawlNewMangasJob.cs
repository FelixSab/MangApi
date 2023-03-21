using API.Cron.Interfaces;
using Interface;
using DB;
using MediatR;
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
        var mediator = _serviceProvider.GetService<IMediator>()!;

        var mangas = await crawler.CrawlRecentlyUpdatedMangas();
        await mediator.UpsertManganatoMangas(mangas);
    }
}
