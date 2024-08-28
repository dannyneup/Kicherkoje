using Kicherkoje.Automations.Shared.Attributes;

namespace Kicherkoje.Automations.Shared.Enums.States;

[HaEntityState]
public enum SunState
{
    [HaStringRepresentation("below_horizon")]
    BelowHorizon,

    [HaStringRepresentation("above_horizon")]
    AboveHorizon
}