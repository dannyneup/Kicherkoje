using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.TestUtilities.Extensions;

public static class HaContextExtensions
{
    public static void TriggerStateChange(this HaContextMock context, Entity entity, string newStateValue, object? attributes = null)
    {
        var newState = new EntityState { State = newStateValue };
        if (attributes != null)
        {
            newState = newState with { AttributesJson = attributes.AsJsonElement() };
        }

        context.TriggerStateChange(entity.EntityId, newState);
    }

    public static void TriggerStateChange(this HaContextMock context, string entityId, EntityState newState)
    {
        var oldState = context.EntityStates.TryGetValue(entityId, out var current) ? current : null;
        context.EntityStates[entityId] = newState;
        context.StateAllChangeSubject.OnNext(new StateChange(new Entity(context, entityId), oldState, newState));
    }

    public static void VerifyServiceCalled(this HaContextMock context, Entity entity, string domain, string service)
    {
        context.Received().CallService(domain, service,
            Arg.Is<ServiceTarget?>(s => s!.EntityIds!.SingleOrDefault() == entity.EntityId),
            Arg.Any<object>());
    }

    public static void TriggerEvent(this HaContextMock context, Event @event)
    {
        context.EventsSubject.OnNext(@event);
    }
}