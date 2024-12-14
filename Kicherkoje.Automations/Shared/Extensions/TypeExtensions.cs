using System.Linq.Expressions;
using System.Reflection;

namespace Kicherkoje.Automations.Shared.Extensions;

public static class TypeExtensions
{
    public static PropertyInfo GetProperty<T, TValue>(this T _, Expression<Func<T, TValue>> selector) where T : class
    {
        var expression = selector.Body;

        return expression.NodeType == ExpressionType.MemberAccess
            ? (PropertyInfo)((MemberExpression)expression).Member
            : throw new ArgumentException("Expression '{SelectorExpression}' refers to a method, not a property.", selector.ToString());
    }
}