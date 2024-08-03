using Kicherkoje.Automations.Unittests.Apps.Utilities.Extensions;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.Apps.Mocks;

public class HaContextMock : HaContextBase, IHaContextMock
{
    public IHaContext Mock { get; } = Substitute.For<IHaContext>();

    public override void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        Mock.CallService(domain, service, target, data);
    }

    public override void SendEvent(string eventType, object? data = null)
    {
        Mock.SendEvent(eventType, data);
    }

    public void VerifyServiceCalled(Entity entity, string domain, string service)
    {
        Mock
            .Received()
            .CallService(
                domain,
                service,
                Arg.Is<ServiceTarget>(n => n.EntityIds!.Contains(entity.EntityId)),
                Arg.Any<object?>()
            );
    }

    public void TriggerStateChange(Entity entity, string newStatevalue, object? attributes = null)
    {
        var newState = new EntityState { State = newStatevalue };
        if (attributes != null)
        {
            newState = newState.WithAttributes(attributes);
        }

        TriggerStateChange(entity.EntityId, newState);
    }

    public void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = EntityStates.TryGetValue(entityId, out var current) ? current : null;
        EntityStates[entityId] = newState;
        StateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));
    }
}