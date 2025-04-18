using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;
using Roguish.Screens;
using SystemsRx.Attributes;

namespace Roguish.ECS.Systems;

// Currently this system is called synchronously when the IsDestroyedComponent is added to an entity.
// That's not exactly what I want but it may not cause any problems.  Until it does I guess I'll leave
// it like this.  Perhaps, ultimately, I want this on a timed update loop.
[Priority(-100)]
// ReSharper disable once UnusedType.Global
internal class SweepUpSystem(DungeonSurface dungeon) : ISetupSystem
{
    public IGroup Group => new Group(typeof(IsDestroyedComponent));
    public void Setup(EcsEntity entity)
    {
        if (entity.HasComponent(typeof(DisplayComponent)))
        {
            dungeon.RemoveScEntity(entity.GetComponent<DisplayComponent>()!.ScEntity);
        }

        EcsApp.EntityDatabase.RemoveEntity(entity);
    }
}