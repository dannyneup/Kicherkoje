using Kicherkoje.Automations.Shared.Scheduler;

namespace Kicherkoje.Automations.Apps.Shared;

public abstract class AppBase
{
    internal readonly IEntities Entities;
    internal readonly IHaContext HaContext;
    internal readonly ILogger Logger;
    internal readonly ISchedulerService SchedulerService;
    internal readonly IServices Services;

    protected AppBase(
        IHaContext haContext,
        ILogger logger,
        ISchedulerService schedulerService)
    {
        HaContext = haContext;
        Logger = logger;
        SchedulerService = schedulerService;
        Entities = new Entities(HaContext);
        Services = new Services(HaContext);


        Logger.LogDebug("Started {Name}", GetType().Name);
    }
}