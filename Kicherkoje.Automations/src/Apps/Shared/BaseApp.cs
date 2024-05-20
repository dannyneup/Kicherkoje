using System.Reactive.Concurrency;

namespace Kicherkoje.Automations.Apps.Shared;

public class BaseApp
{
    internal readonly Entities Entities;
    internal readonly ILogger Logger;
    internal readonly IScheduler Scheduler;
    internal readonly IServices Services;
    internal readonly IHaContext HaContext;

    protected BaseApp(
        IHaContext haContext, 
        ILogger logger, 
        IScheduler scheduler)
    {
        HaContext = haContext;
        Logger = logger;
        Scheduler = scheduler;
        Entities = new Entities(haContext);
        Services = new Services(haContext);

        Logger.LogDebug("Started {Name}", GetType().Name);
    }
}