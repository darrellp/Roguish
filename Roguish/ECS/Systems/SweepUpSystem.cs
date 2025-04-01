using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
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
internal class SweepUpSystem(DungeonSurface dungeon) : IReactToGroupSystem
{
    public IGroup Group => new Group(typeof(IsDestroyedComponent));

    public IObservable<IObservableGroup> ReactToGroup(IObservableGroup observableGroup)
    {
        var group = GetGroup(typeof(IsDestroyedComponent));
        return group.OnEntityAdded.Select(_ => group);
    }

    public void Process(EcsEntity entity)
    {
        if (entity.HasComponent(typeof(DisplayComponent)))
        {
            var scEntity = (entity.GetComponent(typeof(DisplayComponent)) as DisplayComponent)!.ScEntity;
            dungeon.RemoveScEntity(scEntity);
        }

        EcsApp.EntityDatabase.RemoveEntity(entity);
    }
}