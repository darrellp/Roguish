using System.Diagnostics;
using Roguish.ECS.Events;
using Roguish.ECS.Components;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;
using Roguish.Screens;
using Roguish.ECS.Tasks;


namespace Roguish.ECS.Systems;
internal class NewTurnEventSystem : IReactToEventSystem<NewTurnEvent>
{
    private static DungeonSurface _dungeon = null!;
    private static LogScreen _log = null!;
    private static object Lock = new();

    public NewTurnEventSystem(DungeonSurface dungeon, LogScreen log)
    {
        _dungeon = dungeon;
        _log = log;
    }

    public void Process(NewTurnEvent eventData)
    {
        Monitor.Enter(Lock);

        // Get the player's task and achieve it
        var player = EcsRxApp.Player;

        // The Player's first task is special - it is the one that drives all the
        // other tasks in the game because it is driven itself by user input.  The
        // algorithm is:
        //      1. Get the player's first task
        //      2. Achieve that and move the clock ahead according to the time it took
        //      3. Do any other tasks (including the player's) that got triggered as a result
        
        // We don't need to check for null here since this system only gets called when the
        // user has performed some action and set it up as a task.
        var uiDrivenTaskCmp = player.GetComponent<TaskComponent>();
        Debug.Assert(uiDrivenTaskCmp != null);

        var uiDrivenTask = uiDrivenTaskCmp.Tasks[0];
        Debug.Assert(uiDrivenTask != null);

        // We shouldn't be moving BACK in time!
        Debug.Assert(TaskGetter.Ticks <= uiDrivenTask.FireOn);

        TaskGetter.Ticks = uiDrivenTask.FireOn;
        Debug.Assert(uiDrivenTask.Action != null, "uiDrivenTask.Action != null");
        uiDrivenTask.Action(player, uiDrivenTask);

        // Player gets priority in ALL their tasks
        foreach (var nextTask in uiDrivenTaskCmp.Tasks.Skip(1).Where(t => t.FireOn <= TaskGetter.Ticks))
        {
            // This player task is already due so we can just do it
            Debug.Assert(nextTask.Action != null, "nextTask.Action != null");
            nextTask.Action(player, nextTask);
        }
        // Player is special - it will get a new task when the UI demands it
        // so no replacing here
        uiDrivenTaskCmp.Tasks[0] = null!;

        foreach (var tasked in EcsRxApp.TaskedGroup.ToArray())
        {
            if (tasked.HasComponent<IsDestroyedComponent>() || tasked.Id == player.Id)
            {
                continue;
            }
            var taskCmp = tasked.GetComponent<TaskComponent>();
            foreach (var task in taskCmp.Tasks.Where(t => t.FireOn <= TaskGetter.Ticks))
            {
                Debug.Assert(task.Action != null, "task.Action != null");

                while (task.FireOn <= TaskGetter.Ticks)
                {
                    task.Action(tasked, task);
                }
            }
        }
        // Everybody should have moved if they wanted by this time so time to check visibility
        CheckScEntityVisibility();
        Monitor.Exit(Lock);

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
                if (scEntity.IsVisible && ecsEntity.HasComponent<AgentComponent>() && ecsEntity.Id != EcsRxApp.Player.Id && KeyboardEventSystem.HasQueue())
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
