using System.Collections.Generic;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Shared.Extensions;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Children of entity {parentEntity} returned empty.")]
    public static partial void LogNoEntityChildrenFoundMessage(this ILogger logger, Entity parentEntity);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Job with key {jobKey} with job-data {jobData} scheduled for {triggerStartTime}.")]
    public static partial void LogJobScheduled(this ILogger logger, string jobKey, IDictionary<string, object> jobData,
        DateTimeOffset triggerStartTime);
}