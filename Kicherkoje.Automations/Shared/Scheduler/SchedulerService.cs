using System.Threading.Tasks;
using Kicherkoje.Automations.Shared.Extensions;
using Quartz;

namespace Kicherkoje.Automations.Shared.Scheduler;

public interface ISchedulerService
{
    Task InitializeAsync();

    Task ScheduleJobInAsync<TJob>(TimeSpan delay, ConflictBehavior conflictBehavior = ConflictBehavior.ThrowException)
        where TJob : IJob;

    Task ScheduleJobInAsync<TJob, TParam>(TimeSpan delay, TParam jobParameters, ConflictBehavior conflictBehavior = ConflictBehavior.ThrowException)
        where TJob : IParameterizedJob<TParam>
        where TParam : class;

    Task ScheduleDailyJobAsync<TJob>(TimeOnly time,
        ConflictBehavior conflictBehavior = ConflictBehavior.ThrowException)
        where TJob : IJob;

    Task ScheduleDailyJobAsync<TJob, TParam>(TimeOnly time, TParam jobParameters,
        ConflictBehavior conflictBehavior = ConflictBehavior.ThrowException)
        where TJob : IParameterizedJob<TParam>
        where TParam : class;

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

    public Task ScheduleJobInAsync<TJob>(TimeSpan delay,
        ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException)
        where TJob : IJob
        => ScheduleJobInInternalAsync<TJob, object>(delay, null, conflictBehavior);

    public Task ScheduleJobInAsync<TJob, TParam>(TimeSpan delay, TParam jobParameters,
        ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException) where TJob : IParameterizedJob<TParam> where TParam : class
        => ScheduleJobInInternalAsync<TJob, TParam>(delay, jobParameters, conflictBehavior);

    public Task ScheduleDailyJobAsync<TJob, TParam>(TimeOnly time, TParam jobParameters,
        ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException)
        where TJob : IParameterizedJob<TParam>
        where TParam : class
        => ScheduleDailyJobInternalAsync<TJob, TParam>(time, jobParameters, conflictBehavior);

    public Task ScheduleDailyJobAsync<TJob>(TimeOnly time, ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException)
        where TJob : IJob
        => ScheduleDailyJobInternalAsync<TJob, object>(time, null, conflictBehavior);

    private async Task ScheduleJobInInternalAsync<TJob, TParam>(TimeSpan delay, TParam? jopParameters, ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException)
        where TJob : IJob
        where TParam : class
    {
        var (jobKey, baseJobKeyExists) = await GetJobKeyAsync<TJob>(conflictBehavior);

        if (baseJobKeyExists && conflictBehavior is ISchedulerService.ConflictBehavior.KeepExisting)
            return;

        var jobBuilder = JobBuilder
            .Create<TJob>()
            .WithIdentity(jobKey);
        if (jopParameters != null)
            jobBuilder.SetJobData(CreateJobDataMap(jopParameters));
        var job = jobBuilder.Build();

        var trigger = TriggerBuilder
            .Create()
            .ForJob(job)
            .StartAt(DateTimeOffset.Now.Add(delay))
            .Build();

        await ScheduleJobAsync(job, trigger, conflictBehavior);
    }

    private async Task ScheduleDailyJobInternalAsync<TJob, TParam>(TimeOnly time, TParam? jobParameters,
        ISchedulerService.ConflictBehavior conflictBehavior = ISchedulerService.ConflictBehavior.ThrowException)
        where TJob : IJob
        where TParam : class
    {
        var (jobKey, baseJobKeyExists) = await GetJobKeyAsync<TJob>(conflictBehavior);

        if (baseJobKeyExists && conflictBehavior is ISchedulerService.ConflictBehavior.KeepExisting)
            return;

        var jobBuilder = JobBuilder
            .Create<TJob>()
            .WithIdentity(jobKey);
        if (jobParameters != null)
            jobBuilder.SetJobData(CreateJobDataMap(jobParameters));
        var job = jobBuilder.Build();

        var trigger = TriggerBuilder.Create()
            .ForJob(job)
            .WithIdentity("daily")
            .WithDailyTimeIntervalSchedule(builder =>
                builder.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(time.Hour, time.Minute)))
            .Build();

        await ScheduleJobAsync(job, trigger, conflictBehavior);
    }


    private JobDataMap? CreateJobDataMap<TParam>(TParam? jobParameters)
    {
        if (jobParameters == null)
            return null;

        var parameterProperties = typeof(TParam).GetProperties();
        var jobDataMap = new JobDataMap();

        foreach (var parameterProperty in parameterProperties)
        {
            var parameterValue = parameterProperty.GetValue(jobParameters);
            if (parameterValue is null)
            {
                logger.LogError("Property '{ParameterProperty}' has no value", parameterProperty.Name);
                continue;
            }
            jobDataMap.Add(parameterProperty.Name, parameterValue);
        }
        return jobDataMap;
    }

    private async Task ScheduleJobAsync(IJobDetail jobDetail, ITrigger trigger,
        ISchedulerService.ConflictBehavior conflictBehavior)
    {
        var shouldReplace = conflictBehavior is ISchedulerService.ConflictBehavior.OverwriteExisting;
        await Scheduler.ScheduleJob(jobDetail, [trigger], shouldReplace);
        logger.LogJobScheduled(jobDetail.Key.ToString(), jobDetail.JobDataMap.WrappedMap, trigger.StartTimeUtc);
    }

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
