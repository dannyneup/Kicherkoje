namespace Kicherkoje.Automations.Apps.Shared;

public class AppConfig<T>(T value) : IAppConfig<T>
    where T : class, new()
{
    public T Value { get; } = value;
}