using System.Reactive.Subjects;
using System.Text.Json;
using Kicherkoje.Automations.Unittests.TestUtilities.Extensions;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.TestUtilities;

public class HaContextMockBase : IHaContext
{
    private readonly Subject<Event> _eventsSubject = new();
    private readonly Subject<StateChange> _stateAllChangeSubject = new();
    public readonly Dictionary<string, EntityState> EntityStates = new();

    public HaContextMockBase() => Entities = this.LoadGeneratedEntities().ToList();

    public List<Entity> Entities { get; set; }

    public IObservable<StateChange> StateAllChanges() => _stateAllChangeSubject;

    public EntityState? GetState(string entityId) =>
        EntityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => Entities;

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
    }

    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null,
        object? data = null) => throw new NotSupportedException();

    public Area? GetAreaFromEntityId(string entityId) => null;

    public virtual void SendEvent(string eventType, object? data = null)
    {
    }

    public IObservable<Event> Events => _eventsSubject;

    public EntityRegistration GetEntityRegistration(string entityId) => throw new NotSupportedException();

    public void TriggerStateChange(Entity entity, string newStateValue, object? attributes = null)
    {
        var newState = new EntityState { State = newStateValue };
        if (attributes != null) newState = newState with { AttributesJson = attributes.AsJsonElement() };

        TriggerStateChange(entity.EntityId, newState);
    }

    public void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = EntityStates.TryGetValue(entityId, out var current) ? current : null;
        EntityStates[entityId] = newState;
        _stateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));
    }

    public void TriggerEvent(Event @event)
    {
        _eventsSubject.OnNext(@event);
    }

    public virtual void VerifyServiceCalled(Entity entity, string domain, string service, int count = 1)
    {
    }
}