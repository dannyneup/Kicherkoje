namespace Kicherkoje.Automations.Shared.Enumerations.Types;

public abstract class EventType : Enumeration
{
    public static EnumerationItem CallService => new("call_service");
}