using Kicherkoje.Automations.Shared.Attributes;

namespace Kicherkoje.Automations.Shared.Enums.Services;

[HaServiceDomain]
[HaStringRepresentation("light")]
public enum LightService
{
    [HaStringRepresentation("turn_on")] TurnOn,
    [HaStringRepresentation("turn_off")] TurnOff
}