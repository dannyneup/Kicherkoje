using FluentAssertions;
using Kicherkoje.Automations.Configuration.HomeAssistantGenerated;
using Kicherkoje.Automations.Shared.Extensions;
using Kicherkoje.Automations.Unittests.TestUtilities.Extensions;
using NetDaemon.HassModel.Entities;
using NSubstitute;

namespace Kicherkoje.Automations.Unittests.Shared.Extensions;

public class EntityExtensionTests
{
    private readonly IHaContext _haContext;
    private readonly List<LightEntity> _children;
    private readonly List<string> _childrenIds;
    private readonly LightEntity _group;

    public EntityExtensionTests()
    {
        _haContext = Substitute.For<IHaContext>();

        _group = new LightEntity(_haContext, "group");
        _children =
        [
            new LightEntity(_haContext, "firstChild"),
            new LightEntity(_haContext, "secondChild")
        ];
        _childrenIds = _children.Select(entity => entity.EntityId).ToList();

        SetupHaContext();
        return;

        void SetupHaContext()
        {
            var lightAttributes = new LightAttributes { EntityId = _childrenIds };
            var state = new EntityState();
            var stateWithAttributes = new EntityState<LightAttributes>(state)
            {
                AttributesJson = lightAttributes.AsJsonElement()
            };

            _haContext.GetState(_group.EntityId).Returns(stateWithAttributes);
        }
    }

    [Fact]
    public async Task GetChildren_ReturnsGroupsChildren()
    {
        var returnedChildren = _group.GetChildren();
        var returnedChildrenIds = returnedChildren?.Select(entity => entity.EntityId).ToList();

        returnedChildrenIds.Should().BeEquivalentTo(_childrenIds);
    }

    [Fact]
    public void GetChildren_WithHAResponseDelay_ReturnsGroupChildren()
    {
        var result = _haContext.GetState(_group.EntityId);
        _haContext.GetState(_group.EntityId).Returns(_ =>
        {
            Task.Delay(TimeSpan.FromMilliseconds(1000)).Wait();
            return result;
        });

        var returnedChildren = _group.GetChildren();
        var returnedChildrenIds = returnedChildren?.Select(entity => entity.EntityId).ToList();

        returnedChildrenIds.Should().BeEquivalentTo(_childrenIds);
    }
}