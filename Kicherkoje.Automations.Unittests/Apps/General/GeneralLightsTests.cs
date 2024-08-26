using System.Reactive.Concurrency;
using Kicherkoje.Automations.Apps.General;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using Kicherkoje.Automations.Helpers.Enums.Services;
using Kicherkoje.Automations.Helpers.Enums.States;
using Kicherkoje.Automations.Helpers.Extensions;
using Kicherkoje.Automations.Unittests.TestUtilities;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.Apps.General;

public sealed class GeneralLightsTests
{
    private readonly Entities _entities;
    private readonly HaContextMock _haContext;

    public GeneralLightsTests()
    {
        _haContext = new HaContextMock();
        _entities = new Entities(_haContext);
    }

    [Fact]
    public void OnSunRise_TurnOffLights_TurnsOffLightsExceptHallGrowLamp()
    {
        var (app, config) = InitGeneralLights();

        _haContext.TriggerStateChange(_entities.Sun.Sun, SunState.AboveHorizon.GetHaStringRepresentation());

        foreach (var lightEntity in _entities.Light.EnumerateAll())
        {
            if (config.Value.GrowLights.Contains(lightEntity))
                continue;

            _haContext.VerifyServiceCalled(lightEntity, typeof(LightService).GetHaStringRepresentation(),
                LightService.TurnOff.GetHaStringRepresentation());
        }
    }
    
    [Fact]
    public void OnSunRise_TurnOffLights_DoesNotTurnOffGrowLights()
    {
        var (app, config) = InitGeneralLights();

        _haContext.TriggerStateChange(_entities.Sun.Sun, SunState.AboveHorizon.GetHaStringRepresentation());

        foreach (var lightEntity in _entities.Light.EnumerateAll())
        {
            if (config.Value.GrowLights.Contains(lightEntity))
            {
                _haContext.VerifyServiceCalled(lightEntity, typeof(LightService).GetHaStringRepresentation(),
                    Arg.Any<string>(), 0);
            }
        }
    }

    private (GeneralLights app, IAppConfig<GeneralLightsConfig> config) InitGeneralLights(
        IAppConfig<GeneralLightsConfig>? config = null)
    {
        config ??= CreateDefaultConfig();
        return new ValueTuple<GeneralLights, IAppConfig<GeneralLightsConfig>>(
            new GeneralLights(_haContext, Substitute.For<ILogger<GeneralLights>>(), Substitute.For<IScheduler>(),
                config),
            config
        );

        IAppConfig<GeneralLightsConfig> CreateDefaultConfig()
        {
            var growLights = new[]
            {
                _entities.Light.HallGrowLamp,
                _entities.Light.LivingRoomPlantRackLight
            };
            var configMock = Substitute.For<IAppConfig<GeneralLightsConfig>>();
            configMock.Value.Returns(new GeneralLightsConfig(growLights));
            return configMock;
        }
    }
}