using System.Reactive.Subjects;
using System.Text.Json;
using Kicherkoje.Automations.Unittests.Apps.Utilities.Extensions;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.Apps.Mocks;

public class HaContextMock : IHaContextMock
{
    
    public Dictionary<string, EntityState> EntityStates { get; } = new();
    public Subject<Event> EventsSubject { get; } = new();
    public Subject<StateChange> StateAllChangeSubject { get; } = new();
    

    public IObservable<StateChange> StateAllChanges() => StateAllChangeSubject;

    public EntityState? GetState(string entityId) => EntityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => EntityStates.Keys.Select(entityId => new Entity(this, entityId)).ToList();

    public void CallService(string domain, string service, ServiceTarget? target = null, object? data = null) =>
        throw new NotImplementedException();

    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null) => throw new NotImplementedException();

    public Area? GetAreaFromEntityId(string entityId) => throw new NotImplementedException();

    public EntityRegistration? GetEntityRegistration(string entityId) => throw new NotImplementedException();

    public void SendEvent(string eventType, object? data = null) => throw new NotImplementedException();

    public IObservable<Event> Events => EventsSubject;

    public void TriggerStateChange(Entity entity, string newStateValue, DateTime? lastUpdated = null, DateTime? lastChanged = null,
        object? attributes = null)
    {
        var newState = new EntityState { State = newStateValue, LastUpdated = lastUpdated, LastChanged = lastChanged };
        if (attributes != null) newState = newState.WithAttributes(attributes);

        TriggerStateChange(entity.EntityId, newState);
    }

    public void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = EntityStates.TryGetValue(entityId, out var current) ? current : null;
        EntityStates[entityId] = newState;
        StateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));
    }

    public void VerifyServiceCalled(Entity entity, string domain, string service)
    {
        throw new NotImplementedException();
    }
}