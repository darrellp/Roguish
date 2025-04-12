using System.Diagnostics;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;
using Roguish.Screens;

namespace Roguish.ECS.Systems;

// ReSharper disable once UnusedType.Global
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
        ClearLevel();
        // Repopulate the new dungeon
        _dungeon.Populate(eventData.NewLevel);
    }

    internal static void ClearLevel()
    {
        // Get the FOV for the new dungeon
        Fov = new FOV(MapGenerator.WallFloorValues);

        // Delete the old stuff in the old dungeon floor
        foreach (var item in EcsRxApp.LevelItems.ToArray())
        {
            if (item.HasComponent<IsPlayerControlledComponent>())
            {
                continue;
            }
            if (item.HasComponent<DisplayComponent>())
            {
                var displayCmp = item.GetComponent(typeof(DisplayComponent)) as DisplayComponent;
                _dungeon.RemoveScEntity(displayCmp!.ScEntity);
            }

            if (!item.HasComponent<InBackpackComponent>() && !item.HasComponent<IsEquippedComponent>())
            {
                EcsApp.EntityDatabase.GetCollection().RemoveEntity(item.Id);
            }
        }
        MapGenerator.ClearEntityMaps();
    }
}
