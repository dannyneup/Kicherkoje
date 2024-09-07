namespace Kicherkoje.Automations.Shared.Enumerations.States;

public abstract class LightState : Enumeration
{
    public static EnumerationItem On => new("on");
    public static EnumerationItem Off => new("off");
}