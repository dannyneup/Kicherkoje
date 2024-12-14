namespace Kicherkoje.Automations.Apps.StateManagers.Shared;

public interface IStateManager<TState> where TState : class
{
    IObservable<StateChange<TState>> StateChanges();
}