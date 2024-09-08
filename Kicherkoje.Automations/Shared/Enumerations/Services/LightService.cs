namespace Kicherkoje.Automations.Shared.Enumerations.Services;

public abstract class LightService : IServiceEnumeration
{
    public static string TurnOn => "turn_on";
    public static string TurnOff => "turn_off";
    public static string Domain => "light";
}