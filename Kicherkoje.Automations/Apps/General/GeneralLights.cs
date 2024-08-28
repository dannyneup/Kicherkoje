using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Helpers.Enums.States;
using Kicherkoje.Automations.Helpers.Extensions;

namespace Kicherkoje.Automations.Apps.General;

[NetDaemonApp(Id = "GeneralLights")]
public class GeneralLights : AppBase
{
    private readonly IAppConfig<GeneralLightsConfig> _config;


    public GeneralLights(IHaContext context, ILogger<GeneralLights> logger, IScheduler scheduler,
        IAppConfig<GeneralLightsConfig> config) : base(context, logger, scheduler)
    {
        _config = config;

        OnSunRise_TurnOffLights();
        OnGrowLightsTurnOn_ScheduleTurnOff();
    }


    private void OnSunRise_TurnOffLights()
    {
        Entities.Sun.Sun.StateChanges()
            .Where(c => c.New?.State == SunState.AboveHorizon.GetHaStringRepresentation())
            .Subscribe(x =>
                {
                    foreach (var lightEntity in Entities.Light.EnumerateAll())
                    {
                        if (_config.Value.GrowLights.Contains(lightEntity))
                            continue;
                        lightEntity.TurnOff();
                    }
                }
            );
    }

    private void OnGrowLightsTurnOn_ScheduleTurnOff()
    {
        Entities.Light.Growlights
            .StateChanges()
            .Where(s => s.New?.State == LightState.On.GetHaStringRepresentation())
            .Subscribe(_ =>
                Scheduler.Schedule(
                    new TimeSpan(12, 0, 0), () =>
                        Entities.Light.Growlights.TurnOff()
                )
            );
    }
}

public class GeneralLightsConfig : ConfigBase
{
    public GeneralLightsConfig() =>
        GrowLights =
        [
            Entities.Light.LivingRoomPlantRackLight,
            Entities.Light.HallGrowLamp
        ];

    public GeneralLightsConfig(IEnumerable<LightEntity> growLights) => GrowLights = growLights.ToList();

    public IReadOnlyList<LightEntity> GrowLights { get; }
}