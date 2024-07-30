using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.Apps.Mocks;

public interface IHaContextMock : IHaContext
{
    void TriggerStateChange(Entity entity, string newStateValue, DateTime? lastUpdated = null, DateTime? lastChanged = null, object? attributes = null);
    void TriggerStateChange(string entityId, EntityState newState);
    void VerifyServiceCalled(Entity entity, string domain, string service);
}