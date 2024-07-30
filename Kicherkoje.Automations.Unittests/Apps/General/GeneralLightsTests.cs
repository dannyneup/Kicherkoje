using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Kicherkoje.Automations.Apps.General;
using Kicherkoje.Automations.HomeAssistantGenerated;
using Kicherkoje.Automations.Unittests.Apps.Mocks;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.Apps.General;

public sealed class GeneralLightsTests
{
    private readonly Entities _entities;
    private readonly IHaContextMock _haContext;
    private readonly ILogger<GeneralLights> _logger;
    private readonly IScheduler _scheduler;
    private readonly GeneralLights _sut;

    public GeneralLightsTests()
    {
        _haContext = new HaContextMock();
        _logger = Substitute.For<ILogger<GeneralLights>>();
        _scheduler = Substitute.For<IScheduler>();
        _sut = new GeneralLights(_haContext, _logger, _scheduler);
        _entities = new Entities(_haContext);
    }

    [Fact]
    public void OnSunRise_TurnOffLights_TurnsOffLightsExceptHallGrowLamp() { }

    [Fact]
    public void OnLastPersonLeaves_TurnOffLights_TurnsOffLightsExceptHallGrowLamp() { }
}