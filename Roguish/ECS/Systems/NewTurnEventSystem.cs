using System.Diagnostics;
using Roguish.ECS.Events;
using Roguish.ECS.Components;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;
using GoRogue.Random;
using Roguish.Map_Generation;
using Roguish.Screens;
using Roguish.ECS.Tasks;


namespace Roguish.ECS.Systems;
internal class NewTurnEventSystem : IReactToEventSystem<NewTurnEvent>
{
    private static DungeonSurface _dungeon = null!;
    private static LogScreen _log = null!;

    public NewTurnEventSystem(DungeonSurface dungeon, LogScreen log)
    {
        _dungeon = dungeon;
        _log = log;
    }

    public void Process(NewTurnEvent eventData)
    {
        // Get the player's task and achieve it
        var player = EcsRxApp.Player;
        if (!player.HasComponent<TaskComponent>())
        {
            return;
        }
        var taskCmp = player.GetComponent<TaskComponent>();
        Debug.Assert(taskCmp != null);

        // TODO: Figure out why the assertion below fires very occasionally.
        // I suspect some sort of race condition in the keyboard tasks so probably
        // won't see unless going "double" or "triple" speed.  I think on slower
        // machines there's the possibility that publishes can get mixed up and come
        // in the wrong order.  I can't get this to happen on my big fast desktop
        // but I can on my laptop.
        Debug.Assert(TaskGetter.Ticks < taskCmp.FireOn);

        TaskGetter.Ticks = taskCmp.FireOn;
        Debug.Assert(taskCmp.Action != null, "taskCmp.Action != null");
        taskCmp.Action(player);
        // Player is special - it will get a new task when the UI demands it
        // so no replacing here
        player.RemoveComponent<TaskComponent>();

        foreach (var tasked in EcsRxApp.TaskedGroup.ToArray())
        {
            if (tasked.HasComponent<IsDestroyedComponent>())
            {
                continue;
            }
            var task = tasked.GetComponent<TaskComponent>();
            while (task.FireOn <= TaskGetter.Ticks)
            {
                Debug.Assert(task.Action != null, "task.Action != null");
                task.Action(tasked);
            }
        }
        // Everybody should have moved if they wanted by this time so time to check visibility
        CheckScEntityVisibility();

    }

    internal static void CheckScEntityVisibility()
    {
        foreach (var ecsEntity in EcsRxApp.DisplayGroup)
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
                if (scEntity.IsVisible && ecsEntity.HasComponent<AgentComponent>() && ecsEntity.Id != EcsRxApp.Player.Id)
                {
                    var name = Utility.GetName(ecsEntity);
                    _log.PrintProcessedString($"Stopping since [c:r f:Yellow]{name}[c:undo] came into view");
                    // Stopping the player because another agent is in view
                    KeyboardEventSystem.StopQueue();
                    
                }
            }
        }
    }

}
