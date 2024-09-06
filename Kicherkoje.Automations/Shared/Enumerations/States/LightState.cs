namespace Kicherkoje.Automations.Shared.Enumerations.States;

public abstract class LightState : Enumeration
{
    public static readonly EnumerationItem On = new("on");
    public static readonly EnumerationItem Off = new("off");
}