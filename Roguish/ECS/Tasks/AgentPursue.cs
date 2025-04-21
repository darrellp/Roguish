using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.Info;
using Roguish.Map_Generation;
using System;
using System.Diagnostics;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static void AgentPursue(EcsEntity agent, RogueTask task)
    {
        var playerPos = EcsApp.PlayerPos;
        var posCmp = agent.GetComponent<PositionComponent>();
        var agentCmp = agent.GetComponent<AgentTypeComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        var ptMove = pos.Neighbors(GameSettings.DungeonWidth, GameSettings.DungeonHeight, false)
            .Where(Movable)
            .MinBy(p => p.Manhattan(EcsApp.PlayerPos));
        task.FireOn += agentCmp.MoveTime;
        if (BattleCheck(agent, ptMove))
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
            task.TaskType = TaskType.AgentMove;
        }
    }

    private static bool Movable(Point pt)
    {
        return MapGenerator.IsWalkable(pt) && (!MapGenerator.IsAgentAt(pt) || pt == EcsApp.PlayerPos);
    }

    internal static TaskComponent CreateAgentPursueTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + StdMovementTime, TaskType.AgentPursue);
    }
}
