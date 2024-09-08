using System.Reactive.Concurrency;
using Kicherkoje.Automations.Apps.General;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using Kicherkoje.Automations.Shared.Enumerations.Services;
using Kicherkoje.Automations.Shared.Enumerations.States;
using Kicherkoje.Automations.Unittests.TestUtilities;
using Kicherkoje.Automations.Unittests.TestUtilities.Extensions;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
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
        var config = InitGeneralLights();

        _haContext.TriggerStateChange(_entities.Sun.Sun, SunState.AboveHorizon);

        foreach (var lightEntity in _entities.Light.EnumerateAll())
            if (!config.GrowLightEntities.Contains(lightEntity))
                _haContext.VerifyServiceCalled(LightService.Domain,
                    LightService.TurnOff, lightEntity);
    }

    [Fact]
    public void OnSunRise_TurnOffLights_DoesNotTurnOffGrowLights()
    {
        var config = InitGeneralLights();

        _haContext.TriggerStateChange(_entities.Sun.Sun, SunState.AboveHorizon);

        foreach (var lightEntity in _entities.Light.EnumerateAll())
            if (config.GrowLightEntities.Contains(lightEntity))
                _haContext.VerifyServiceCalled(LightService.Domain,
                    Arg.Any<string>(), lightEntity, 0);
    }

    private GeneralLightsMockConfig InitGeneralLights()
    {
        var growLightEntities = SetUpGrowLightEntityIdAttributes();
        _ = new GeneralLights(_haContext, Substitute.For<ILogger<GeneralLights>>(), Substitute.For<IScheduler>());

        return new GeneralLightsMockConfig(growLightEntities);
    }

    private List<LightEntity> SetUpGrowLightEntityIdAttributes()
    {
        var growLightGroup = _entities.Light.Growlights;
        var growLightGroupId = growLightGroup.EntityId;

        var growLightChildren = new List<LightEntity>
            { _entities.Light.HallGrowLamp, _entities.Light.LivingRoomPlantRackLight };
        var growLightChildrenIds = growLightChildren.Select(g => g.EntityId).ToList();
        var lightAttributes = new LightAttributes { EntityId = growLightChildrenIds };

        var state = new EntityState { EntityId = growLightGroupId };
        var stateWithAttributes = new EntityState<LightAttributes>(state)
            { AttributesJson = lightAttributes.AsJsonElement() };

        _haContext.EntityStates[growLightGroupId] = stateWithAttributes;

        var growLightEntities = growLightChildren.Append(growLightGroup);
        return growLightEntities.ToList();
    }

    private record GeneralLightsMockConfig(List<LightEntity> GrowLightEntities);
}