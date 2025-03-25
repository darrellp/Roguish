using System.Diagnostics;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;

// ReSharper disable once UnusedMember.Global
internal class NewDungeonSystem : IReactToEventSystem<NewDungeonEvent>
{
    private readonly MapGenerator _mapgen;
    static DungeonSurface _dungeon = null!;

    public NewDungeonSystem(MapGenerator mapgen, DungeonSurface dungeon)
    {
        _mapgen = mapgen;
        _dungeon = dungeon;
    }

    public void Process(NewDungeonEvent eventData)
    {
        Fov = new FOV(_mapgen.WallFloorValues);
        foreach (var item in EcsApp.LevelItems)
        {
            if (item.HasComponent(typeof(IsPlayerControlledComponent)) && item.HasComponent(typeof(DisplayComponent)))
            {
                if (item.HasComponent(typeof(PositionComponent)))
                {
                    var posCmp = item.GetComponent(typeof(PositionComponent)) as PositionComponent;
                    Debug.Assert(posCmp != null, nameof(posCmp) + " != null");
                    posCmp.FDrawFullFov = true;
                    Debug.Assert(_dungeon != null, nameof(_dungeon) + " != null");
                    posCmp!.Position.SetValueAndForceNotify(_dungeon.FindRandomEmptyPoint());
                }
            }
        }
        _dungeon.Populate(eventData.NewLevel);
    }
}
