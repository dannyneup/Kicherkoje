using Kicherkoje.Automations.Apps.System;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using Kicherkoje.Automations.Shared.Enumerations.Services;
using Kicherkoje.Automations.Shared.Scheduler;
using Kicherkoje.Automations.Unittests.TestUtilities;
using Microsoft.Extensions.Logging;

namespace Kicherkoje.Automations.Unittests.Apps.System;

public class NotificationRelayAppTests
{
    private readonly HaContextMock _haContext;
    private readonly Services _services;

    public NotificationRelayAppTests()
    {
        _haContext = new HaContextMock();
        _services = new Services(_haContext);
    }

    [Theory]
    [InlineData("testTitle", "testMessage", "testId")]
    [InlineData(null, "testMessage", "testId")]
    [InlineData("testTitle", "testMessage", null)]
    [InlineData(null, "testMessage", null)]
    public void PersistentNotificationCreateServiceCalled_TriggersNotificationWithCorrectData(string? title,
        string message, string? id)
    {
        InitializeNotificationRelayApp();

        _services.PersistentNotification.Create(new PersistentNotificationCreateParameters
        {
            Message = message,
            Title = title,
            NotificationId = id
        });

        title ??= string.Empty;
        _haContext.Mock.Received().CallService(NotifyService.Domain, NotifyService.MobileAppIPhoneVonDanny,
            data: Arg.Is<NotifyMobileAppIphoneVonDannyParameters>(parameters =>
                parameters.Title == title && parameters.Message == message));
    }

    [Theory]
    [InlineData("testTitle", "testMessage")]
    [InlineData(null, "testMessage")]
    public void NotifyPersistentNotificationServiceCalled_ValidData_TriggersNotificationWithCorrectData(string? title,
        string message)
    {
        InitializeNotificationRelayApp();

        _services.Notify.PersistentNotification(new NotifyPersistentNotificationParameters
        {
            Message = message,
            Title = title
        });

        title ??= string.Empty;
        _haContext.Mock.Received().CallService(NotifyService.Domain, NotifyService.MobileAppIPhoneVonDanny,
            data: Arg.Is<NotifyMobileAppIphoneVonDannyParameters>(parameters =>
                parameters.Title == title && parameters.Message == message));
    }


    private NotificationRelayApp InitializeNotificationRelayApp() =>
        new(_haContext, Substitute.For<ILogger<NotificationRelayApp>>(), Substitute.For<ISchedulerService>());
}