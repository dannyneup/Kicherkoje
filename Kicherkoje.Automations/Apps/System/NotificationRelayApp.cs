using System.Reactive.Linq;
using System.Text.Json;
using Kicherkoje.Automations.Apps.Shared;
using Kicherkoje.Automations.Shared.Enumerations.Types;
using Kicherkoje.Automations.Shared.Scheduler;

namespace Kicherkoje.Automations.Apps.System;

[NetDaemonApp]
public class NotificationRelayApp : AppBase
{
    public NotificationRelayApp(IHaContext context, ILogger<NotificationRelayApp> logger, ISchedulerService schedulerService) : base(
        context, logger, schedulerService)
    {
        OnPersistantNotificationCreated_CreatePushNotification();
    }

    private void OnPersistantNotificationCreated_CreatePushNotification()
    {
        HaContext.Events
            .Where(IsPersistentNotificationServiceCall)
            .Subscribe(e =>
            {
                var notificationData = TryGetNotificationData(e);
                if (notificationData is null)
                    throw new NullReferenceException(
                        $"Could not read notification data from event {e}");

                Services.Notify.MobileAppIphoneVonDanny(new NotifyMobileAppIphoneVonDannyParameters
                {
                    Message = notificationData.Value.message,
                    Title = notificationData.Value.title
                });
            });

        return;

        bool IsPersistentNotificationServiceCall(Event @event)
        {
            if (@event.EventType != EventType.CallService)
                return false;

            var serviceProperties = TryGetServiceProperties(@event);
            return serviceProperties is { domain: "persistent_notification", service: "create" } or
                { domain: "notify", service: "persistent_notification" };
        }

        (string domain, string service)? TryGetServiceProperties(Event @event) =>
            @event.DataElement?.TryGetProperty("domain", out var domain) == true
            && @event.DataElement?.TryGetProperty("service", out var service) == true
                ? (domain.ToString(), service.ToString())
                : null;

        (string title, string message)? TryGetNotificationData(Event @event)
        {
            var serviceData = new JsonElement();
            if (@event.DataElement?.TryGetProperty("service_data", out serviceData) == false)
                return null;
            if (!serviceData.TryGetProperty("message", out var message))
                return null;
            serviceData.TryGetProperty("title", out var title);

            return (title.ToString(), message.ToString());
        }
    }
}