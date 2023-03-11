using Quartz;

namespace API.Cron.Interfaces;

public interface ICronJob : IJob
{
    public static abstract string CronSchedule { get; }
    public static abstract string Name { get; }
}
