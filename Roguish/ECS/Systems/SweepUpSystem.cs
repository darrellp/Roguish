using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Systems;
using Roguish.ECS.Components;
using SystemsRx.Attributes;

namespace Roguish.ECS.Systems;

// Currently this system is called synchronously when the IsDestroyedComponent is added to an entity.
// That's not exactly what I want but it may not cause any problems.  Until it does I guess I'll leave
// it like this.  Perhaps, ultimately, I want this on a timed update loop.
[Priority(-100)]
internal class SweepUpSystem : IReactToGroupSystem
{
    private DungeonSurface _dungeon;
    public IGroup Group => new Group(typeof(IsDestroyedComponent));

    public SweepUpSystem(DungeonSurface dungeon)
    {
        _dungeon = dungeon;
    }

    public IObservable<IObservableGroup> ReactToGroup(IObservableGroup observableGroup)
    {
        var group = Program.GetGroup(typeof(IsDestroyedComponent));
        return group.OnEntityAdded.Select(_ => group);
    }

    public void Process(EcsEntity entity)
    {
        if (entity.HasComponent(typeof(DisplayComponent)))
        {
            var scEntity = ((entity.GetComponent(typeof(DisplayComponent)) as DisplayComponent)!).ScEntity;
            _dungeon.RemoveScEntity(scEntity);
        }
        Program.EcsApp.EntityDatabase.RemoveEntity(entity);
    }
}
