namespace Kicherkoje.Automations.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
public class HaStringRepresentationAttribute(string state) : Attribute
{
    public string State { get; } = state;
}