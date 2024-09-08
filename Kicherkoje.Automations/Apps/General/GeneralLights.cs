using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Shared.Enumerations.States;
using Kicherkoje.Automations.Shared.Extensions;

namespace Kicherkoje.Automations.Apps.General;

[NetDaemonApp(Id = "GeneralLights")]
public class GeneralLights : AppBase
{
    public GeneralLights(IHaContext context, ILogger<GeneralLights> logger, IScheduler scheduler) : base(context,
        logger, scheduler)
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
                    var growLightEntities = growLightsGroup.GetChildren() ?? [];
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
            .Subscribe(_ =>
                Scheduler.Schedule(
                    new TimeSpan(12, 0, 0), () =>
                        Entities.Light.Growlights.TurnOff()
                )
            );
    }
}