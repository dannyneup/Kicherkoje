using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Kicherkoje.Automations.Apps.Shared;

namespace Kicherkoje.Automations.Apps.General;

[NetDaemonApp(Id = "GeneralLights")]
public class GeneralLights : AppBase
{
    private readonly List<LightEntity> _allLights;

    public GeneralLights(IHaContext haContext, ILogger<GeneralLights> logger, IScheduler scheduler) : base(haContext, logger, scheduler)
    {
        _allLights = Entities.Light.EnumerateAll().ToList();
    }

    private void OnSunRise_TurnOffLights()
    {
        Entities.Sun.Sun.StateChanges()
            .Where(c => c.New?.State == "above_horizon")
            .Subscribe(x => 
                _allLights.Where(l => l != Entities.Light.HallGrowLamp)
                    .ToList()
                    .ForEach(e => e.TurnOff())
                );
    }
    
    private void OnLastPersonLeaves_TurnOffLights()
    {
        Entities.Sun.Sun.StateChanges()
            .Where(c => c.New?.State == "above_horizon")
            .Subscribe(x => 
                _allLights.Where(l => l != Entities.Light.HallGrowLamp)
                    .ToList()
                    .ForEach(e => e.TurnOff())
            );
    }
}