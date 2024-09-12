using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Shared.Extensions;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Warning,
        Message = "Children of entity {parentEntity} returned empty.")]
    public static partial void LogNoEntityChildrenFoundMessage(this ILogger logger, Entity parentEntity);
}