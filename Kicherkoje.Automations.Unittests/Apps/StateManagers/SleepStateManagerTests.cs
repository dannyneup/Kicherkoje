using System.Reactive.Concurrency;
using FluentAssertions;
using Kicherkoje.Automations.Apps.StateManagers;
using Kicherkoje.Automations.Apps.StateManagers.Shared;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using Kicherkoje.Automations.Shared.Enumerations;
using Kicherkoje.Automations.Shared.Enumerations.States;
using Kicherkoje.Automations.Unittests.TestUtilities;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.Apps.StateManagers;

public class SleepStateManagerTests
{
    private readonly HaContextMock _context;
    private readonly Entities _entities;

    public SleepStateManagerTests()
    {
        _context = new HaContextMock();
        _entities = new Entities(_context);
    }

    public static TheoryData<Dictionary<Resident, (string isFocusEnabled, string activeFocusMode)>, List<Resident>>
        TestData =>
        new()
        {
            {
                new Dictionary<Resident, (string isFocusEnabled, string activeFocusMode)>
                {
                    { Resident.Danny, (BinaryState.On, FocusMode.Sleep) },
                    { Resident.Liv, (BinaryState.On, FocusMode.Sleep) }
                },
                [Resident.Danny, Resident.Liv]
            },
            {
                new Dictionary<Resident, (string isFocusEnabled, string activeFocusMode)>
                {
                    { Resident.Danny, (BinaryState.Off, FocusMode.Sleep) },
                    { Resident.Liv, (BinaryState.On, FocusMode.Sleep) }
                },
                [Resident.Liv]
            },
            {
                new Dictionary<Resident, (string isFocusEnabled, string activeFocusMode)>
                {
                    { Resident.Danny, (BinaryState.On, FocusMode.Work) },
                    { Resident.Liv, (BinaryState.On, FocusMode.Sleep) }
                },
                [Resident.Liv]
            },
            {
                new Dictionary<Resident, (string isFocusEnabled, string activeFocusMode)>
                {
                    { Resident.Danny, (BinaryState.On, FocusMode.Work) },
                    { Resident.Liv, (BinaryState.Off, FocusMode.Sleep) }
                },
                []
            }
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public void FocusEntityChanged_UpdatesSleepStateCorrectly(
        Dictionary<Resident, (string isFocusEnabled, string activeFocusMode)> residentsFocusInformation,
        List<Resident> expectedSleepingResidents)
    {
        SetupContext();
        var sut = new SleepStateManager(_context, Substitute.For<ILogger>(), Substitute.For<IScheduler>());
        var action = Substitute.For<Action<StateChange<SleepState>>>();
        sut.StateChanges().Subscribe(action);

        _context.TriggerStateChange(_entities.BinarySensor.IphoneVonDannyFocus,
            residentsFocusInformation[Resident.Danny].isFocusEnabled);

        action.Received(1).Invoke(Arg.Is<StateChange<SleepState>>(change =>
            change.NewState.SleepingResidents.Count == expectedSleepingResidents.Count));
        return;

        void SetupContext()
        {
            var livFocusStateEntityId = _entities.BinarySensor.IphoneVonLivGreteFocus.EntityId;
            var dannyFocusModeEntityId = _entities.InputSelect.DannyFocus.EntityId;
            var livFocusModeEntityId = _entities.InputSelect.LivFocus.EntityId;

            residentsFocusInformation
                .TryGetValue(Resident.Danny, out var dannyFocusInformation)
                .Should()
                .BeTrue();

            residentsFocusInformation
                .TryGetValue(Resident.Liv, out var livFocusInformation)
                .Should()
                .BeTrue();

            _context.EntityStates[livFocusStateEntityId] =
                new EntityState { State = livFocusInformation.isFocusEnabled };
            _context.EntityStates[dannyFocusModeEntityId] =
                new EntityState { State = dannyFocusInformation.activeFocusMode };
            _context.EntityStates[livFocusModeEntityId] =
                new EntityState { State = livFocusInformation.activeFocusMode };
        }
    }

    [Theory]
    [InlineData("binary_sensor.iphone_von_danny_focus")]
    [InlineData("binary_sensor.iphone_von_liv_grete_focus")]
    public void FocusStateEntityChanged_TriggersSleepStateChange(string entityId)
    {
        var sut = new SleepStateManager(_context, Substitute.For<ILogger>(), Substitute.For<IScheduler>());
        var action = Substitute.For<Action<StateChange<SleepState>>>();
        sut.StateChanges().Subscribe(action);

        _context.TriggerStateChange(_context.Entity(entityId), "on");

        action.Received(1).Invoke(Arg.Any<StateChange<SleepState>>());
    }

    [Theory]
    [InlineData("input_select.danny_focus")]
    [InlineData("input_select.liv_focus")]
    public void FocusModeEntityChanged_TriggersSleepStateChange(string entityId)
    {
        var sut = new SleepStateManager(_context, Substitute.For<ILogger>(), Substitute.For<IScheduler>());
        var action = Substitute.For<Action<StateChange<SleepState>>>();
        sut.StateChanges().Subscribe(action);

        _context.TriggerStateChange(_context.Entity(entityId), FocusMode.Sleep);

        action.Received(1).Invoke(Arg.Any<StateChange<SleepState>>());
    }
}