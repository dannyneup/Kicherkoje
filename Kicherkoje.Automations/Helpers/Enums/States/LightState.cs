using Kicherkoje.Automations.Helpers.Attributes;

namespace Kicherkoje.Automations.Helpers.Enums.States;

[HaEntityState]
public enum LightState
{
    [HaStringRepresentation("on")] On,

    [HaStringRepresentation("off")] Off
}