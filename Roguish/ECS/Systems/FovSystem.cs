using System.Diagnostics;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;
using SystemsRx.Scheduling;

namespace Roguish.ECS.Systems;
internal class FovSystem(DungeonSurface dungeon) : IBasicEntitySystem
{
    public IGroup Group => new Group(typeof(CellFovComponent));

    public void Process(EcsEntity entity, ElapsedTime elapsedTime)
    {
        var fovCmp = entity.GetComponent(typeof(CellFovComponent)) as CellFovComponent;
        Debug.Assert(fovCmp != null);
        if (fovCmp.IsInFov)
        {
            dungeon.MarkSeen(fovCmp.CellPos);
        }
        else
        {
            dungeon.MarkUnseen(fovCmp.CellPos);
        }
        EcsApp.EntityDatabase.GetCollection().RemoveEntity(entity.Id);
    }

    public static void SignalNewFov()
    {
        // Do we need to clear out any old CellFov entities?
        var collection = EcsApp.EntityDatabase.GetCollection();
        foreach (var point in Fov.NewlySeen)
        {
            var entity = collection.CreateEntity();
            entity.AddComponent(new CellFovComponent(true, point));
        }
        foreach (var point in Fov.NewlyUnseen)
        {
            var entity = collection.CreateEntity();
            entity.AddComponent(new CellFovComponent(false, point));
        }
    }
}
