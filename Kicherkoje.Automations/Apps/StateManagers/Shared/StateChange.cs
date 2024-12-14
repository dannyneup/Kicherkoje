using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kicherkoje.Automations.Apps.StateManagers.Shared;

public class StateChange<TPropertyValue>(IEnumerable<PropertyInfo> changedProperties, TPropertyValue? oldState, TPropertyValue newState)
    where TPropertyValue : class
{
    public IReadOnlyList<PropertyInfo> ChangedProperties { get; } = changedProperties.ToList();

    public TPropertyValue? OldState { get; } = oldState;
    public TPropertyValue NewState { get; } = newState;
}