using System.Diagnostics;
using Roguish.ECS.Events;
using Roguish.ECS.Components;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;
using GoRogue.Random;
using Roguish.Map_Generation;
using Roguish.Screens;


namespace Roguish.ECS.Systems;
internal class NewTurnEventSystem : IReactToEventSystem<NewTurnEvent>
{
    private static DungeonSurface _dungeon;
    internal static ulong Ticks { get; set; }

    public NewTurnEventSystem(DungeonSurface dungeon)
    {
        _dungeon = dungeon;
    }

    public void Process(NewTurnEvent eventData)
    {
        // Get the player's task and achieve it
        var player = EcsApp.PlayerGroup[0];
        if (!player.HasComponent<TaskComponent>())
        {
            return;
        }
        var taskCmp = player.GetComponent<TaskComponent>();
        Debug.Assert(taskCmp != null);
        // TODO: Figure out why this assertion fires very occasionally.
        // I suspect some sort of race condition in the keyboard tasks so probably
        // won't see unless going "double" or "triple" speed
        Debug.Assert(Ticks < taskCmp.FireOn);
        Ticks = taskCmp.FireOn;
        Debug.Assert(taskCmp.Action != null, "taskCmp.Action != null");
        taskCmp.Action(player);
        // Player is special - it will get a new task when the UI demands it
        // so no replacing here
        player.RemoveComponent<TaskComponent>();

        foreach (var tasked in EcsApp.TaskedGroup.ToArray())
        {
            var task = tasked.GetComponent<TaskComponent>();
            while (task.FireOn <= Ticks)
            {
                Debug.Assert(task.Action != null, "task.Action != null");
                task.Action(tasked);
            }
        }
        // Everybody should have moved if they wanted by this time so time to check visibility
        CheckScEntityVisibility();

    }

    internal static void DefaultAgentMove(EcsEntity enemy)
    {
        var posCmp = enemy.GetComponent<PositionComponent>();
        var agentCmp = enemy.GetComponent<AgentComponent>();
        var taskCmp = enemy.GetComponent<TaskComponent>();
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

    internal static void CheckScEntityVisibility()
    {
        foreach (var ecsEntity in EcsApp.DisplayGroup)
        {
            var scEntity = ecsEntity.GetComponent<DisplayComponent>().ScEntity;
            if (!_dungeon.DrawFov)
            {
                scEntity.IsVisible = true;
                continue;
            }
            var playerDelta = scEntity.Position - EcsApp.PlayerPos;
            var deltaModulus = playerDelta.X * playerDelta.X + playerDelta.Y * playerDelta.Y;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (deltaModulus > GameSettings.FovRadius * GameSettings.FovRadius)
            {
                scEntity.IsVisible = false;
            }
            else
            {
                scEntity.IsVisible = Fov.CurrentFOV.Contains(scEntity.Position);
            }
        }
    }

}
