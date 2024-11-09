using System.Threading.Tasks;
using Kicherkoje.Automations.Shared.Extensions;
using Quartz;

namespace Kicherkoje.Automations.Shared.Services;

public interface ISchedulerService
{
    public Task ScheduleJobAsync<TJob>(TimeSpan delay, bool replace = false) where TJob : IJob;
}


public class SchedulerService : ISchedulerService
{
    private readonly Lazy<Task<IScheduler>> _scheduler;
    private readonly ILogger<SchedulerService> _logger;

    public SchedulerService(ISchedulerFactory schedulerFactory, ILogger<SchedulerService> logger)
    {
        _scheduler = new Lazy<Task<IScheduler>>(async () =>
        {
            var scheduler = await schedulerFactory.GetScheduler();
            await scheduler.Start();
            return scheduler;
        });
        _logger = logger;
    }

    public async Task ScheduleJobAsync<TJob>(TimeSpan delay, bool replace = false) where TJob : IJob
    {
        var scheduler = await _scheduler.Value;

        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.Now.Add(delay))
            .Build();

        var jobKey = await GetJobKeyAsync<TJob>(scheduler, replace);

        var job = JobBuilder.Create<TJob>()
            .WithIdentity(jobKey)
            .Build();

        await scheduler.ScheduleJob(job, [trigger], replace);
        _logger.LogJobScheduled(job.JobType, job.JobDataMap.WrappedMap, trigger.StartTimeUtc);
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