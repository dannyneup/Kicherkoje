using Kicherkoje.Automations.Apps.General;
using Kicherkoje.Automations.Helpers.Enums.Services;
using Kicherkoje.Automations.Helpers.Enums.States;
using Kicherkoje.Automations.Helpers.Extensions;
using Kicherkoje.Automations.HomeAssistantGenerated;
using Kicherkoje.Automations.Unittests.TestUtilities;
using Kicherkoje.Automations.Unittests.TestUtilities.Extensions;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.Apps.General;

public sealed class GeneralLightsTests
{
    private readonly HaContextMock _haContext;
    private readonly Entities _entities;

    public GeneralLightsTests()
    {
        _haContext = Substitute.For<HaContextMock>();
        _entities = new Entities(_haContext);
    }

    [Fact]
    public void OnSunRise_TurnOffLights_TurnsOffLightsExceptHallGrowLamp()
    {
        InitGeneralLights();
        
        _haContext.TriggerStateChange(_entities.Light.HallGrowLamp, LightService.TurnOn.GetHaStringRepresentation());
        
        _haContext.VerifyServiceCalled(_entities.Light.SigneGradientTable1, typeof(LightService).GetHaStringRepresentation(), LightService.TurnOff.GetHaStringRepresentation());
        
    }

    private GeneralLights InitGeneralLights()
    {
        return new GeneralLights(_entities);
    }
}