using API.Cron.Interfaces;
using API.Cron.Jobs;
using Quartz;
using Quartz.Impl;
using static Quartz.Logging.OperationName;

namespace API.Cron;

public static class Extensions
{
    public static IServiceCollection AddQuartzConfigured(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            RegisterCronJob<CrawlNewMangasJob>();

            // ----- Local functions -----

            void RegisterCronJob<TJob>()
                where TJob : ICronJob
            {
                var jobKey = new JobKey(TJob.Name);
                q.AddJob<TJob>(jobKey);


                var identity = GetJobIdentity<TJob>();


                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity(GetJobIdentity<TJob>())
                    .WithCronSchedule(TJob.CronSchedule));
            }

        });

        return services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });
    }

    private static string GetJobIdentity<TJob>()
        where TJob : ICronJob
        => $"{TJob.Name}-trigger";
}
