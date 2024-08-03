using System.Reactive.Concurrency;

namespace Kicherkoje.Automations.Apps.Shared;

public class AppBase
{
    internal readonly IEntities Entities;
    internal readonly ILogger Logger;
    internal readonly IScheduler Scheduler;
    internal readonly IServices Services;
    internal readonly IHaContext HaContext;

    protected AppBase(
        IHaContext haContext, 
        IEntities entities,
        IServices services,
        ILogger logger, 
        IScheduler scheduler)
    {
        HaContext = haContext;
        Logger = logger;
        Scheduler = scheduler;
        Entities = entities;
        Services = services;

        Logger.LogDebug("Started {Name}", GetType().Name);
    }
}