using System.Collections.Generic;
using System.Linq;
using NetDaemon.HassModel.Entities;

namespace Kicherkoje.Automations.Shared.Extensions;

public static class EntityExtensions
{
    public static List<Entity>? GetChildren(this LightEntity entity) =>
        entity.GetChildren<LightEntity>();

    private static List<Entity>? GetChildren<TEntity>(this TEntity entity)
        where TEntity : Entity
    {
        var entityState = entity.EntityState;
        var entityIds = (entityState?.Attributes as dynamic)?.EntityId as List<string>;
        var entities = entityIds?.Select(entityId => entity.HaContext.Entity(entityId)).ToList();
        return entities;
    }
}