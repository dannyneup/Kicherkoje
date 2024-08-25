using Kicherkoje.Automations.Helpers.Attributes;

namespace Kicherkoje.Automations.Helpers.Enums.States;

[HaEntityState]
public enum SunState
{
    [HaStringRepresentation("below_horizon")]
    BelowHorizon,
    [HaStringRepresentation("above_horizon")]
    AboveHorizon
}