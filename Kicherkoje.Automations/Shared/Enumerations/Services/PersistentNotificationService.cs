namespace Kicherkoje.Automations.Shared.Enumerations.Services;

public abstract class PersistentNotificationService : IServiceEnumeration
{
    public static string Create => "create";
    public static string Dismiss => "dismiss";
    public static string MarkRead => "mark_read";
    public static string Domain => "persistent_notification";
}