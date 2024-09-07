namespace Kicherkoje.Automations.Shared.Enumerations.Services;

public class LightService : IServiceEnumeration
{
    public static EnumerationItem TurnOn => new("turn_on");
    public static EnumerationItem TurnOff => new("turn_off");
    public static EnumerationItem Domain => new("light");
}