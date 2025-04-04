﻿using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using System.Diagnostics;
using EcsRx.Extensions;
using Ninject;
using Roguish.Screens;
using GoRogue.Random;

namespace Roguish.ECS;
internal static class Tasks
{
    #region Fields
    internal static ulong Ticks { get; set; } = 0;
    private static DungeonSurface _dungeon;
    private static MapGenerator _mapgen;
    private static LogScreen _log;
    private static InventorySurface _inv;
    #endregion

    #region Task Times
    internal const int StdMovementTime = 100;
    internal const int PickUpTime = 10;
    internal const int EquipTime = 10;
    #endregion

    #region (static) Constructor
    static Tasks()
    {
        _dungeon = Kernel.Get<DungeonSurface>();
        _mapgen = Kernel.Get<MapGenerator>();
        _log = Kernel.Get<LogScreen>();
        _inv = Kernel.Get<InventorySurface>();
    }
    #endregion

    #region Player Movement
    private static Action<EcsEntity> MovePlayerClosure(Point newPosition)
    {
        return _ => { MovePlayer(newPosition); };
    }

    private static void MovePlayer(Point newPosition)
    {
        var player = EcsRxApp.Player;
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        Debug.Assert(MapGenerator.IsWalkable(newPosition));
        positionCmp.Position.SetValueAndForceNotify(newPosition);
        _dungeon.KeepPlayerInView();
    }

    internal static TaskComponent CreatePlayerMoveTask(Point newPt, ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + StdMovementTime, MovePlayerClosure(newPt));
    }
    #endregion

    #region Agent Movement
    internal static void DefaultAgentMove(EcsEntity agent)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var agentCmp = agent.GetComponent<AgentComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        var moves = pos.
            Neighbors(GameSettings.DungeonWidth, GameSettings.DungeonHeight, false).
            Where(MapGenerator.IsWalkable).
            ToArray();
        posCmp.Position.Value = moves[GlobalRandom.DefaultRNG.NextInt(moves.Length)];
        taskCmp.FireOn += agentCmp.MoveTime;
        // Don't need this right now since it's not changing
        // taskCmp.Action = DefaultMonsterMove;
    }

    internal static TaskComponent CreateAgentMoveTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + StdMovementTime, DefaultAgentMove);
    }
    #endregion

    #region Picking up
    internal static void UserPickup(EcsEntity agent)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        // TODO: Handle fMore being true here
        var (entity, fMore) = _mapgen.GetEntityAt(pos, true);
        if (entity != null && !entity.HasComponent<AgentComponent>())
        {
            if (entity.HasComponent<DisplayComponent>())
            {
                var displayCmp = entity.GetComponent<DisplayComponent>();
                displayCmp.ScEntity.IsVisible = false;
                _dungeon.RemoveScEntity(displayCmp.ScEntity);
                _mapgen.RemoveItemAt(pos, entity.Id);
            }

            var name = Utility.GetName(entity);
            
            //"an [c:r f:Yellow]unknown object[c:undo]");
            //if (entity.HasComponent<DescriptionComponent>())
            //{
            //    name = entity.GetComponent<DescriptionComponent>().Name;
            //    name = Utility.PrefixWithAorAnColored(name.ToLower(), "Yellow");
            //}

            _log.PrintProcessedString($"Picked up {name}");
            entity.RemoveComponent<PositionComponent>();
            entity.AddComponent<InBackpackComponent>();
        }
        else
        {
            _log.Cursor.Print("There's nothing to be picked up here").NewLine();
        }

        if (!agent.HasComponent<IsPlayerControlledComponent>())
        {
            agent.AddComponent(CreateAgentMoveTask(taskCmp.FireOn));
        }
    }

    internal static TaskComponent CreatePickupTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }

        return new(currentTicks + PickUpTime, UserPickup);
    }
    #endregion

    #region Equipping
    internal static TaskComponent CreateEquipTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + EquipTime, UserEquip);
    }

    public static void UserEquip(EcsEntity agent)
    {
        _inv.Equip();
    }

    #endregion

}
