namespace Kicherkoje.Automations.Apps.StateManagers.Shared;

public class StateChange<T>(T? oldState, T newState)
    where T : class
{
    public T? OldState { get; } = oldState;
    public T NewState { get; } = newState;
}