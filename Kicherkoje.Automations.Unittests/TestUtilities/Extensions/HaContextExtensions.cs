using System.Reflection;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Unittests.TestUtilities.Extensions;

public static class HaContextExtensions
{
    public static IEnumerable<Entity> LoadGeneratedEntities(this IHaContext context)
    {
        var entities = new Entities(context);
        
        var properties = entities.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create a list to hold all entities
        var allEntities = new List<Entity>();

        foreach (var property in properties)
        {
            // Get the value of the property which should be a domain class (e.g., Lights, Sensors)
            var domain = property.GetValue(entities);

            if (domain == null)
                continue;

            // Get all properties from the domain class (e.g., all lights)
            var domainProperties = domain.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var domainProperty in domainProperties)
            {
                // Each domain property should be an Entity
                if (domainProperty.GetValue(domain) is Entity entity)
                {
                    allEntities.Add(entity);
                }
            }
        }

        return allEntities.AsEnumerable();
    }
}