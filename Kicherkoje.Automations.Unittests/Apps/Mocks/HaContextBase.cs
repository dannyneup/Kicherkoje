using System.Reactive.Subjects;
using System.Text.Json;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.Apps.Mocks;

public class HaContextBase : IHaContext
{
    internal readonly Dictionary<string, EntityState> EntityStates = new();
    internal readonly Subject<StateChange> StateAllChangeSubject = new();
    private readonly Subject<Event> _eventsSubject = new();

    public IObservable<StateChange> StateAllChanges() => StateAllChangeSubject;

    public EntityState? GetState(string entityId) => EntityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => EntityStates.Keys.Select(s => new Entity(this, s)).ToList();

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null) { }

    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null) => Task.FromResult<JsonElement?>(null);

    public Area? GetAreaFromEntityId(string entityId) => null;

    public virtual void SendEvent(string eventType, object? data = null)
    { }

    public IObservable<Event> Events => _eventsSubject;

    public EntityRegistration? GetEntityRegistration(string entityId) => null;
}