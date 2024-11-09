using System.Collections.Generic;
using System.Reactive.Subjects;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Apps.StateManagers.Shared;
using Kicherkoje.Automations.Shared.Enumerations;
using Kicherkoje.Automations.Shared.Enumerations.States;
using Kicherkoje.Automations.Shared.Services;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Apps.StateManagers;

public interface ISleepStateManager
{
    IObservable<StateChange<SleepState>> StateChanges();
}

[NetDaemonApp]
public class SleepStateManager : AppBase, ISleepStateManager
{
    private readonly Subject<StateChange<SleepState>> _stateChanges = new();
    private SleepState? _currentState;

    public SleepStateManager(IHaContext haContext, ILogger logger, ISchedulerService schedulerService) : base(haContext, logger,
        schedulerService)
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

    private Dictionary<Resident, (BinarySensorEntity focusState, InputSelectEntity focusMode)> FocusData => new()
    {
        { Resident.Danny, (Entities.BinarySensor.IphoneVonDannyFocus, Entities.InputSelect.DannyFocus) },
        { Resident.Liv, (Entities.BinarySensor.IphoneVonLivGreteFocus, Entities.InputSelect.LivFocus) }
    };

    public IObservable<StateChange<SleepState>> StateChanges() => _stateChanges;

    private void UpdateSleepState()
    {
        var sleepingResidents = new List<Resident>();
        foreach (var focusDataSet in FocusData)
            if (IsSleepModeActive(focusDataSet.Value.focusState, focusDataSet.Value.focusMode))
                sleepingResidents.Add(focusDataSet.Key);

        var newState = new SleepState(sleepingResidents);
        _stateChanges.OnNext(new StateChange<SleepState>(_currentState, newState));
        _currentState = newState;
        return;

        bool IsSleepModeActive(BinarySensorEntity focusState, InputSelectEntity focusMode) =>
            focusState.IsOn() && focusMode.State == FocusMode.Sleep;
    }
}

public record SleepState(List<Resident> SleepingResidents);