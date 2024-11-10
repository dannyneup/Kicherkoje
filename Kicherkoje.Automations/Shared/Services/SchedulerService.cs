using System.Threading.Tasks;
using Quartz;

namespace Kicherkoje.Automations.Shared.Services;

public interface ISchedulerService
{
    Task InitializeAsync();
    Task ScheduleJobAsync<TJob>(TimeSpan delay, bool replace = false) where TJob : IJob;
}

public class SchedulerService(ISchedulerFactory schedulerFactory, ILogger<SchedulerService> logger)
    : ISchedulerService
{
    private IScheduler? _scheduler;
    private IScheduler Scheduler => _scheduler ?? throw new NullReferenceException("Scheduler is not yet initialized.");

    public async Task InitializeAsync()
    {
        _scheduler = await schedulerFactory.GetScheduler();
        await _scheduler.Start();
    }

    public async Task ScheduleJobAsync<TJob>(TimeSpan delay, bool replace = false) where TJob : IJob
    {
        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.Now.Add(delay))
            .Build();

        var jobKey = await GetJobKeyAsync<TJob>(Scheduler, replace);

        var job = JobBuilder.Create<TJob>()
            .WithIdentity(jobKey)
            .Build();

        await Scheduler.ScheduleJob(job, [trigger], replace);
        logger.LogInformation($"Job '{jobKey}' scheduled to run at {trigger.StartTimeUtc}.");
    }

    private static async Task<JobKey> GetJobKeyAsync<TJob>(IScheduler scheduler, bool replace) where TJob : IJob
    {
        var baseJobKey = new JobKey(typeof(TJob).Name);

        if (replace)
            return baseJobKey;

        var i = 1;
        var jobKey = baseJobKey;
        while (await scheduler.CheckExists(jobKey))
        {
            jobKey = new JobKey($"{baseJobKey.Name}_{i}");
            i++;
        }

        return jobKey;
    }
}
