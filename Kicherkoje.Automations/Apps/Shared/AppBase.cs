using System.Reactive.Concurrency;

namespace Kicherkoje.Automations.Apps.Shared;

public abstract class AppBase
{
    internal readonly IEntities Entities;
    internal readonly IHaContext HaContext;
    internal readonly ILogger Logger;
    internal readonly IScheduler Scheduler;
    internal readonly IServices Services;

    protected AppBase(
        IHaContext haContext,
        ILogger logger,
        IScheduler scheduler)
    {
        HaContext = haContext;
        Logger = logger;
        Scheduler = scheduler;
        Entities = new Entities(HaContext);
        Services = new Services(HaContext);


        Logger.LogDebug("Started {Name}", GetType().Name);
    }
}