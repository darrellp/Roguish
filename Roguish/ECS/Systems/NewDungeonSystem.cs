using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;
using Roguish.Screens;
// ReSharper disable IdentifierTypo

namespace Roguish.ECS.Systems;

// ReSharper disable once UnusedType.Global
internal class NewDungeonSystem : IReactToEventSystem<NewDungeonEvent>
{
    static DungeonSurface _dungeon = null!;

    public NewDungeonSystem(DungeonSurface dungeon)
    {
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
        Fov = new FOV(MapGenerator.WalkableMap);

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
