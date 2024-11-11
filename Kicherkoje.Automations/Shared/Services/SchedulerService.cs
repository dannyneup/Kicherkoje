using System.Threading.Tasks;
using Kicherkoje.Automations.Shared.Extensions;
using Quartz;

namespace Kicherkoje.Automations.Shared.Services;

public interface ISchedulerService
{
    Task InitializeAsync();
    Task ScheduleJobAsync<TJob>(TimeSpan delay, ConflictBehavior conflictBehavior = ConflictBehavior.ThrowException) where TJob : IJob;

    public enum ConflictBehavior
    {
        ThrowException,
        CreateNew,
        OverwriteExisting,
        KeepExisting
    }
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

    public async Task ScheduleJobAsync<TJob>(TimeSpan delay, ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException) where TJob : IJob
    {
        var (jobKey, baseJobKeyExists) = await GetJobKeyAsync<TJob>(conflictBehavior);

        if (baseJobKeyExists && conflictBehavior is ISchedulerService.ConflictBehavior.KeepExisting)
            return;

        var job = BuildJobDetail<TJob>(jobKey);
        var trigger = BuildDelayTrigger(job, delay);

        await ScheduleJobAsync(job, trigger, conflictBehavior);
    }

    private async Task ScheduleJobAsync(IJobDetail jobDetail, ITrigger trigger,
        ISchedulerService.ConflictBehavior conflictBehavior)
    {
        var shouldReplace = conflictBehavior is ISchedulerService.ConflictBehavior.OverwriteExisting;
        await Scheduler.ScheduleJob(jobDetail, [trigger], shouldReplace);
        logger.LogJobScheduled(jobDetail.Key.ToString(), jobDetail.JobDataMap.WrappedMap, trigger.StartTimeUtc);
    }

    private static IJobDetail BuildJobDetail<TJob>(JobKey jobKey) where TJob : IJob =>
        JobBuilder.Create<TJob>()
            .WithIdentity(jobKey)
            .Build();

    private static ITrigger BuildDelayTrigger(IJobDetail jobDetail, TimeSpan delay) =>
        TriggerBuilder.Create()
            .ForJob(jobDetail)
            .StartAt(DateTimeOffset.Now.Add(delay))
            .Build();

    private async Task<(JobKey key, bool keyAlreadyExists)> GetJobKeyAsync<TJob>(ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException, string? key = null) where TJob : IJob
    {
        var baseJobKey = new JobKey(typeof(TJob).Name);

        if (conflictBehavior is ISchedulerService.ConflictBehavior.CreateNew)
            return await GenerateUniqueJobKeyAsync(baseJobKey);

        return (baseJobKey, await Scheduler.CheckExists(baseJobKey));
    }

    private async Task<(JobKey key, bool keyAlreadyExists)> GenerateUniqueJobKeyAsync(JobKey baseJobKey)
    {
        var postfix = 1;
        var jobKey = baseJobKey;
        while (await Scheduler.CheckExists(jobKey))
        {
            jobKey = new JobKey($"{baseJobKey.Name}_{postfix}");
            postfix++;
        }

        return (jobKey, false);
    }
}
