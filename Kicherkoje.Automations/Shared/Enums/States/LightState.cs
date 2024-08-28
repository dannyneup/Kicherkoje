using Kicherkoje.Automations.Shared.Attributes;

namespace Kicherkoje.Automations.Shared.Enums.States;

[HaEntityState]
public enum LightState
{
    [HaStringRepresentation("on")] On,

    [HaStringRepresentation("off")] Off
}