using System.Reactive.Subjects;
using System.Text.Json;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.TestUtilities
{
    public class HaContextMock : IHaContext
    {
        public Dictionary<string, EntityState> EntityStates { get; } = new();
        public Subject<StateChange> StateAllChangeSubject { get; } = new();
        public Subject<Event> EventsSubject { get; } = new();

        public IObservable<StateChange> StateAllChanges() => StateAllChangeSubject;

        public EntityState? GetState(string entityId) => EntityStates.TryGetValue(entityId, out var result) ? result : null;

        public IReadOnlyList<Entity> GetAllEntities() => EntityStates.Keys.Select(s => new Entity(this, s)).ToList();

        public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
        { }

        public Area? GetAreaFromEntityId(string entityId) => null;
        
        public EntityRegistration? GetEntityRegistration(string entityId) => throw new NotImplementedException();

        public virtual void SendEvent(string eventType, object? data = null)
        { }

        public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null)
        {
            throw new NotImplementedException();
        }

        public IObservable<Event> Events => EventsSubject;
    }
}
