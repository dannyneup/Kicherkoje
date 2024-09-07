namespace Kicherkoje.Automations.Shared.Enumerations.States;

public abstract class SunState : Enumeration
{
    public static EnumerationItem BelowHorizon => new("below_horizon");
    public static EnumerationItem AboveHorizon => new("above_horizon");
}