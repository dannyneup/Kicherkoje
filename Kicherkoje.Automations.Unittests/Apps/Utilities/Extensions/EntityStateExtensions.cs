using System.Reflection;
using System.Text.Json;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.Apps.Utilities.Extensions;

public static class EntityStateExtensions
{
    public static EntityState WithAttributes(this EntityState entityState, object attributes)
    {
        var copy = entityState with { };
        entityState.GetType().GetProperty("AttributesJson", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(
            copy, AsJsonElement(attributes));
        return copy;
    }
    
    private static JsonElement AsJsonElement(this object value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }
}