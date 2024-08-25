using Kicherkoje.Automations.Helpers.Attributes;

namespace Kicherkoje.Automations.Helpers.Enums.Services;

[HaServiceDomain]
[HaStringRepresentation("light")]
public enum LightService
{
    [HaStringRepresentation("turn_on")]
    TurnOn,
    [HaStringRepresentation("turn_off")]
    TurnOff,
}