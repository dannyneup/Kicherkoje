using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kicherkoje.Automations.Shared.Enumerations;

public abstract class Enumeration
{
    public static IEnumerable<T> GetAll<T>() where T : EnumerationItem =>
        typeof(T).GetFields(BindingFlags.Public)
            .Select(f => f.GetValue(null))
            .Cast<T>();
}

public class EnumerationItem(string name) : IEquatable<EnumerationItem>
{
    public string Name => name;

    public bool Equals(EnumerationItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EnumerationItem)obj);
    }

    public override int GetHashCode() => Name.GetHashCode();
}