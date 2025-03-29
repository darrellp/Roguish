﻿using System.Diagnostics;
using GoRogue.DiceNotation.Terms;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;

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
        // Get the FOV for the new dungeon
        Fov = new FOV(MapGenerator.WallFloorValues);

        // Delete the old stuff in the old dungeon floor
        foreach (var item in EcsApp.LevelItems.ToArray())
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

            EcsApp.EntityDatabase.GetCollection().RemoveEntity(item.Id);
        }

        // Set up the player in their new position
        var player = EcsApp.PlayerGroup[0];
        var posCmp = player.GetComponent<PositionComponent>();
        Debug.Assert(posCmp != null, nameof(posCmp) + " != null");
        posCmp.FDrawFullFov = true;
        Debug.Assert(_dungeon != null, nameof(_dungeon) + " != null");
        posCmp!.Position.SetValueAndForceNotify(_mapgen.FindRandomEmptyPoint());

        // Repopulate the new dungeon
        _dungeon.Populate(eventData.NewLevel);
    }
}
