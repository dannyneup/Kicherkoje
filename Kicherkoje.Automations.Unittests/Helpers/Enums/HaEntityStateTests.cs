using System.Reflection;
using Kicherkoje.Automations.Shared.Attributes;

namespace Kicherkoje.Automations.Unittests.Helpers.Enums;

public class HaEntityStateTests
{
    [Fact]
    public void AllEnumsWithHaEntityStateShouldHaveHaStringRepresentationOnAllFields()
    {
        var enumTypes = GetAllHaEntityStateEnums();

        foreach (var enumType in enumTypes)
        {
            var fieldsWithoutStringRepresentation = enumType.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field => new
                {
                    Field = field,
                    Attribute = field.GetCustomAttributes(typeof(HaStringRepresentationAttribute))
                        .Cast<HaStringRepresentationAttribute>()
                        .FirstOrDefault()
                })
                .Where(x => x.Attribute == null || string.IsNullOrWhiteSpace(x.Attribute.State))
                .ToList();


            Assert.False(fieldsWithoutStringRepresentation.Count != 0,
                $"The following fields in enum type '{enumType.Name}' have no valid string representation: " +
                $"{string.Join(", ", fieldsWithoutStringRepresentation.Select(x => x.Field.Name))}");
        }
    }

    private static IEnumerable<Type> GetAllHaEntityStateEnums()
    {
        var types = Assembly.GetExecutingAssembly()
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .SelectMany(assembly => assembly.GetTypes());

        return types
            .Where(type => type.IsEnum &&
                           type.CustomAttributes.Any(attribute =>
                               attribute.AttributeType == typeof(HaEntityStateAttribute)
                           )
            );
    }
}