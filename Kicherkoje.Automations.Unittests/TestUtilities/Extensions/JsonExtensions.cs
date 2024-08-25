using System.Text.Json;

namespace Kicherkoje.Automations.Unittests.TestUtilities.Extensions;

public static class JsonExtensions
{
    public static JsonElement AsJsonElement(this object value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }
}