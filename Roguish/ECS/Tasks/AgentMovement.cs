using EcsRx.Extensions;
using GoRogue.Random;
using Roguish.ECS.Components;
using Roguish.Map_Generation;

namespace Roguish.ECS.Tasks;

internal partial class TaskGetter
{
    internal static void DefaultAgentMove(EcsEntity agent, RogueTask task)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var agentCmp = agent.GetComponent<AgentComponent>();
        agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        var moves = pos.Neighbors(GameSettings.DungeonWidth, GameSettings.DungeonHeight, false)
            .Where(MapGenerator.IsWalkable).ToArray();
        posCmp.Position.Value = moves[GlobalRandom.DefaultRNG.NextInt(moves.Length)];
        task.FireOn += agentCmp.MoveTime;
        if (posCmp.Position.Value.Manhattan(EcsApp.PlayerPos) < GameSettings.PursueRadius)
        {
            task.TaskType = TaskType.AgentPursue;
        }
    }

    internal static TaskComponent CreateAgentMoveTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }

        return new(currentTicks + StdMovementTime, TaskType.AgentMove);
    }
}


