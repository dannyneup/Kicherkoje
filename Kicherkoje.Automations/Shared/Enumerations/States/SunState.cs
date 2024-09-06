namespace Kicherkoje.Automations.Shared.Enumerations.States;

public abstract class SunState : Enumeration
{
    public static readonly EnumerationItem BelowHorizon = new("below_horizon");
    public static readonly EnumerationItem AboveHorizon = new("above_horizon");
}