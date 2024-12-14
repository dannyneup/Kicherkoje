using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Shared.Enumerations.States;
using Kicherkoje.Automations.Shared.Extensions;
using Kicherkoje.Automations.Shared.Scheduler;
using Quartz;

namespace Kicherkoje.Automations.Apps.General;

[NetDaemonApp(Id = "GeneralLights")]
public class GeneralLights : AppBase
{
    public GeneralLights(IHaContext context, ILogger<GeneralLights> logger, ISchedulerService schedulerService) : base(
        context,
        logger, schedulerService)
    {
        OnSunRise_TurnOffLightsExceptGrowLights();
        OnGrowLightsTurnOn_ScheduleTurnOff();
    }


    private void OnSunRise_TurnOffLightsExceptGrowLights()
    {
        Entities.Sun.Sun.StateChanges()
            .Where(c => c.New?.State == SunState.AboveHorizon)
            .Subscribe(x =>
                {
                    var growLightsGroup = Entities.Light.Growlights;
                    var growLightEntities = growLightsGroup.GetChildren(Logger);

                    growLightEntities.Add(growLightsGroup);

                    var growLightEntityIds = growLightEntities.Select(entity => entity.EntityId).ToList();
                    var allLights = Entities.Light.EnumerateAll().ToList();

                    var allLightsExceptGrowLights = allLights
                        .Where(light => !growLightEntityIds.Contains(light.EntityId))
                        .ToList();

                    allLightsExceptGrowLights.ForEach(light => light.TurnOff());
                }
            );
    }

    private void OnGrowLightsTurnOn_ScheduleTurnOff()
    {
        Entities.Light.Growlights
            .StateChanges()
            .Where(s => s.New?.State == LightState.On)
            .SubscribeAsync(async _ =>
                await SchedulerService.ScheduleJobInAsync<TrunOffGrowLights>(TimeSpan.FromHours(12), ISchedulerService.ConflictBehavior.KeepExisting)
            );
    }

    private class TrunOffGrowLights(IHaContext haContext) : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var entities = new Entities(haContext);
            entities.Light.Growlights.TurnOff();
            return Task.CompletedTask;
        }
    }
}
