using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Apps.StateManagers.Shared;
using Kicherkoje.Automations.Shared.Enumerations;
using Kicherkoje.Automations.Shared.Enumerations.States;
using Kicherkoje.Automations.Shared.Extensions;
using Kicherkoje.Automations.Shared.Scheduler;
using NetDaemon.HassModel.Entities;
using Quartz;

namespace Kicherkoje.Automations.Apps.StateManagers;

public record HomeState(Daytime? Daytime = null, IReadOnlyList<Resident>? SleepingResidents = null);

[NetDaemonApp]
public class HomeStateManager(IHaContext haContext, ILogger<HomeStateManager> logger, ISchedulerService schedulerService) : AppBase(haContext, logger, schedulerService), IStateManager<HomeState>, IAsyncInitializable
{
    private readonly Subject<StateChange<HomeState>> _stateChanges = new();
    private HomeState _currentState = new();

    private readonly Dictionary<Daytime, TimeOnly> _daytimeMappings = new()
    {
        { Daytime.Morning, new TimeOnly(8, 0) },
        { Daytime.Noon, new TimeOnly(12, 0) },
        { Daytime.Afternoon, new TimeOnly(18, 0) },
        { Daytime.Night, new TimeOnly(22, 0) }
    };

    private Dictionary<Resident, (BinarySensorEntity focusState, InputSelectEntity focusMode)> FocusEntityMapping => new()
    {
        { Resident.Danny, (Entities.BinarySensor.IphoneVonDannyFocus, Entities.InputSelect.DannyFocus) },
        { Resident.Liv, (Entities.BinarySensor.IphoneVonLivGreteFocus, Entities.InputSelect.LivFocus) }
    };

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        SubscribeSleepRelatedEntitiesChanged();

        foreach (var (daytime, time) in _daytimeMappings)
            await SchedulerService.ScheduleDailyJobAsync<UpdateDaytimeJob, UpdateDaytimeJobParameters>(time, new UpdateDaytimeJobParameters(daytime, UpdateDaytime));
    }

    public IObservable<StateChange<HomeState>> StateChanges() => _stateChanges;

    private void SubscribeSleepRelatedEntitiesChanged()
    {
        var entitiesOfInterest = new List<Entity>
        {
            Entities.BinarySensor.IphoneVonDannyFocus,
            Entities.BinarySensor.IphoneVonLivGreteFocus,
            Entities.InputSelect.DannyFocus,
            Entities.InputSelect.LivFocus
        };

        foreach (var entity in entitiesOfInterest)
            entity.StateChanges().Subscribe(_ => UpdateSleepState());
    }

    private void UpdateSleepState()
    {
        var sleepingResidents = new List<Resident>();
        foreach (var focusDataSet in FocusEntityMapping)
        {
            var isSleepModeActive = focusDataSet.Value.focusState.IsOn() &&
                                    focusDataSet.Value.focusMode.State == FocusMode.Sleep;
            if (isSleepModeActive)
                sleepingResidents.Add(focusDataSet.Key);
        }

        var newState = _currentState with { SleepingResidents = sleepingResidents };
        var changedProperties = new List<PropertyInfo> { newState.GetProperty(state => state.SleepingResidents) };

        InvokeNewState(changedProperties, newState);
    }

    private abstract class UpdateDaytimeJob : IParameterizedJob<UpdateDaytimeJobParameters>
    {
        public Task Execute(IJobExecutionContext context)
        {
            var dayTime = (Daytime)context.MergedJobDataMap[nameof(UpdateDaytimeJobParameters.Daytime)];
            var callback = (Action<Daytime>)context.MergedJobDataMap[nameof(UpdateDaytimeJobParameters.OnExecute)];

            callback(dayTime);

            return Task.CompletedTask;
        }
    }

    private void UpdateDaytime(Daytime newDaytime)
    {
        var newState = _currentState with { Daytime = newDaytime};
        var changedProperties = new List<PropertyInfo> { newState.GetProperty(state => state.Daytime) };

        InvokeNewState(changedProperties, newState);
    }

    private void InvokeNewState(List<PropertyInfo> changedProperties, HomeState newState)
    {
        _stateChanges.OnNext(new StateChange<HomeState>(changedProperties ,_currentState, newState));
        _currentState = newState;
    }

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
    private record UpdateDaytimeJobParameters(Daytime Daytime, Action<Daytime> OnExecute);
}

