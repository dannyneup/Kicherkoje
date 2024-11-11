using System.Reactive.Subjects;
using System.Text.Json;
using Kicherkoje.Automations.Unittests.TestUtilities.Extensions;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.TestUtilities;

public class HaContextMockBase : IHaContext
{
    private readonly Subject<Event> _eventsSubject = new();
    private readonly Subject<StateChange> _stateAllChangeSubject = new();
    public Dictionary<string, EntityState> EntityStates { get; } = new();

    public IObservable<StateChange> StateAllChanges() => _stateAllChangeSubject;

    public EntityState? GetState(string entityId) =>
        EntityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => EntityStates.Keys.Select(s => new Entity(this, s)).ToList();

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        var eventData = new Dictionary<string, object?>
        {
            { "domain", domain },
            { "service", service },
            { "target", target },
            { "service_data", data }
        };

        SendEvent("call_service", eventData);
    }

    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null,
        object? data = null) => throw new NotSupportedException();

    public Area? GetAreaFromEntityId(string entityId) => null;

    public virtual void SendEvent(string eventType, object? data = null)
    {
        var @event = new Event
        {
            EventType = eventType,
            DataElement = data?.AsJsonElement() ?? default
        };

        _eventsSubject.OnNext(@event);
    }

    public IObservable<Event> Events => _eventsSubject;

    public EntityRegistration GetEntityRegistration(string entityId) => throw new NotSupportedException();

    public void TriggerStateChange(IEntityCore entity, string newStateValue, object? attributes = null)
    {
        var newState = new EntityState { State = newStateValue };
        if (attributes != null) newState = newState with { AttributesJson = attributes.AsJsonElement() };

        TriggerStateChange(entity.EntityId, newState);
    }

    private void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = EntityStates.TryGetValue(entityId, out var current) ? current : null;
        EntityStates[entityId] = newState;

        _stateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));

        var eventData = new Dictionary<string, object?>
        {
            { "entity_id", entityId },
            { "old_state", oldState?.AsJsonElement() ?? JsonDocument.Parse("{}").RootElement },
            { "new_state", newState.AsJsonElement() }
        };

        SendEvent("state_changed", eventData);
    }

    public void TriggerEvent(Event @event) =>
        _eventsSubject.OnNext(@event);
}