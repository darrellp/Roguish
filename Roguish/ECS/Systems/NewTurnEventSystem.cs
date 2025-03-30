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
    private DungeonSurface _dungeon = null!;
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
        taskCmp.FireOn = Ticks + agentCmp.MoveTime;
        // Don't need this right now since it's not changing
        // taskCmp.Action = DefaultMonsterMove;
    }
}
