using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Helpers.Enums.States;
using Kicherkoje.Automations.Helpers.Extensions;

namespace Kicherkoje.Automations.Apps.General;

[NetDaemonApp(Id = "GeneralLights")]
public class GeneralLights
{
    private readonly IEntities _entities;

    public GeneralLights(IEntities entities)
    {
        _entities = entities;
    }

    private void OnSunRise_TurnOffLights()
    {
        _entities.Light.HallGrowLamp
            .StateChanges()
            .Subscribe(x =>
            {
                _entities.Light.SigneGradientTable1.TurnOff();
            });
    }
}