using System.Reflection;
using Kicherkoje.Automations.Helpers.Attributes;

namespace Kicherkoje.Automations.Helpers.Extensions;

public static class EnumExtensions
{
    public static string GetHaStringRepresentation(this Enum value)
    {
        var enumType = value.GetType();
        var field = enumType.GetField(value.ToString())!;

        if (GetHaStringRepresentationAttributeValue(field) is { } haStringRepresentation)
            return haStringRepresentation;
        
        throw new InvalidOperationException($"Enum value '{value}' of type {enumType} does not have a {typeof(HaStringRepresentationAttribute)} attribute.");
    }

    public static string GetHaStringRepresentation(this Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum.", nameof(enumType));
        
        if (GetHaStringRepresentationAttributeValue(enumType) is { } haStringRepresentation)
            return haStringRepresentation;

        throw new InvalidOperationException($"Enum type '{enumType}' does not have a {typeof(HaStringRepresentationAttribute)} attribute.");
    }
    
    private static string? GetHaStringRepresentationAttributeValue(MemberInfo field) =>
        field.IsDefined(typeof(HaStringRepresentationAttribute)) 
            ? ((HaStringRepresentationAttribute) field.GetCustomAttribute(typeof(HaStringRepresentationAttribute))!).State 
            : null;
}