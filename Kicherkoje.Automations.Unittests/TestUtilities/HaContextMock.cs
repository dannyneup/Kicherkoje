using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.TestUtilities;

public class HaContextMock : HaContextMockBase
{
    public IHaContext Mock { get; } = Substitute.For<IHaContext>();

    public override void CallService(string domain, string service, ServiceTarget? target = null, object? data = null) => 
        Mock.CallService(domain, service, target, data);

    public override void SendEvent(string eventType, object? data = null) => 
        Mock.SendEvent(eventType, data);

    public override void VerifyServiceCalled(Entity entity, string domain, string service, int count = 1) =>
        Mock
            .Received(count)
            .CallService(
                domain,
                service,
                Arg.Is<ServiceTarget>(n => n.EntityIds!.Contains(entity.EntityId)),
                Arg.Any<object?>()
            );
}