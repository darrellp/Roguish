using EcsRx.Extensions;
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
        if (ptMove.Manhattan(EcsApp.PlayerPos) < pos.Manhattan(playerPos))
        {
            posCmp.Position.Value = ptMove;
        }
        else
        {
            ptMove = pos;
        }
        taskCmp.FireOn += agentCmp.MoveTime;
        if (ptMove.Manhattan(playerPos) >= GameSettings.PursueRadius)
        {
            taskCmp.Action = DefaultAgentMove;
        }
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
