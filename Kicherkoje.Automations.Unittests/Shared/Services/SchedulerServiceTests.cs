using Kicherkoje.Automations.Shared.Scheduler;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;

namespace Kicherkoje.Automations.Unittests.Shared.Services;

public class SchedulerServiceTests
{
    private readonly IScheduler _scheduler;
    private readonly SchedulerService _sut;

    public SchedulerServiceTests()
    {
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        _scheduler = Substitute.For<IScheduler>();
        var logger = Substitute.For<ILogger<SchedulerService>>();

        schedulerFactory.GetScheduler().Returns(_scheduler);

        _sut = new SchedulerService(schedulerFactory, logger);
    }

    [Fact]
    public async Task InitializeAsync_StartsScheduler()
    {
        await _sut.InitializeAsync();

        await _scheduler.Received(1).Start();
    }

    [Fact]
    public async Task ScheduleJobAsync_WithDelay_KeyDoesNotExist_SchedulesJob()
    {
        await _sut.InitializeAsync();
        var delay = TimeSpan.FromMinutes(1);
        _scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        await _sut.ScheduleJobInAsync<TestJob>(delay);

        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<IReadOnlyCollection<ITrigger>>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task ScheduleJobAsync_WithDelay_KeepExistingAndKeyExists_DoesNotSchedule()
    {
        await _sut.InitializeAsync();
        var delay = TimeSpan.FromMinutes(1);
        _scheduler.CheckExists(Arg.Any<JobKey>()).Returns(true);

        await _sut.ScheduleJobInAsync<TestJob>(delay, ISchedulerService.ConflictBehavior.KeepExisting);

        await _scheduler.DidNotReceive().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<IReadOnlyCollection<ITrigger>>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task ScheduleJobAsync_WithDelay_OverrideExistingAndKeyExists_SchedulesJobWithReplace()
    {
        await _sut.InitializeAsync();
        var delay = TimeSpan.FromMinutes(1);
        _scheduler.CheckExists(Arg.Any<JobKey>()).Returns(true);

        await _sut.ScheduleJobInAsync<TestJob>(delay, ISchedulerService.ConflictBehavior.OverwriteExisting);

        await _scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<IReadOnlyCollection<ITrigger>>(), true);
    }

    [Fact]
    public async Task ScheduleJobAsync_WithDelay_CreateNewAndKeyExists_SchedulesJobWithCorrectPostfix()
    {
        await _sut.InitializeAsync();
        var delay = TimeSpan.FromMinutes(1);
        _scheduler.CheckExists(Arg.Any<JobKey>()).Returns(true, true, false);

        await _sut.ScheduleJobInAsync<TestJob>(delay, ISchedulerService.ConflictBehavior.CreateNew);

        await _scheduler.Received(1).ScheduleJob(Arg.Is<IJobDetail>(jobDetail => jobDetail.Key.Name == "TestJob_2"), Arg.Any<IReadOnlyCollection<ITrigger>>(), Arg.Any<bool>());
    }
}


public abstract class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context) =>
        Task.CompletedTask;
}