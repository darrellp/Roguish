﻿using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.Map_Generation;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static void AgentPursue(EcsEntity agent)
    {
        var playerPos = EcsApp.PlayerPos;
        var posCmp = agent.GetComponent<PositionComponent>();
        var agentCmp = agent.GetComponent<AgentComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        var ptMove = pos.Neighbors(GameSettings.DungeonWidth, GameSettings.DungeonHeight, false)
            .Where(MapGenerator.IsWalkable)
            .MinBy(p => p.Manhattan(EcsApp.PlayerPos));
        taskCmp.FireOn += agentCmp.MoveTime;
        if (AgentBattleCheck(agent, ptMove))
        {
            return;
        }
        if (ptMove.Manhattan(EcsApp.PlayerPos) < pos.Manhattan(playerPos))
        {
            posCmp.Position.Value = ptMove;
        }
        else
        {
            ptMove = pos;
        }
        if (ptMove.Manhattan(playerPos) >= GameSettings.PursueRadius)
        {
            taskCmp.Action = DefaultAgentMove;
        }
    }
    private static bool AgentBattleCheck(EcsEntity enemy, Point ptDest)
    {
        if (EcsApp.PlayerPos != ptDest)
        {
            // Takes two to tango...
            return false;
        }
        // TODO: MUCH more complicated battle algorithm here!
        var enemyHealthCmp = EcsRxApp.Player.GetComponent<HealthComponent>();
        var enemyName = enemy.GetComponent<DescriptionComponent>().Name;
        var newHealth = Math.Max(0, enemyHealthCmp.CurrentHealth.Value - 3);
        Log.PrintProcessedString($"The [c:r f:Yellow]{enemyName}[c:undo] hits you for 3 damage!");
        if (newHealth == 0)
        {
            Log.PrintProcessedString("[c:r f:Red]*** Y O U   D I E D ! ! ! ***");
            Log.PrintProcessedString("[c:r f:Yellow]***   Resetting because we love you! ***");
            newHealth = 20;
        }
        enemyHealthCmp.CurrentHealth.SetValueAndForceNotify(newHealth);
        return true;
    }


    internal static TaskComponent CreateAgentPursueTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + StdMovementTime, AgentPursue);
    }
}
