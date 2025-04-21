using Roguish.ECS.Components;
using Roguish.Map_Generation;
using System.Diagnostics;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static RogueTask CreatePlayerMoveTask(Point ptDest, ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + StdMovementTime, TaskType.PlayerMove, ptDest);
    }

    public static void MovePlayer(EcsEntity agent, RogueTask t)
    {
        var player = EcsRxApp.Player;
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        if (positionCmp.Position.Value == t.PointArg)
        {
            return;
        }
        Debug.Assert(MapGenerator.IsWalkable(t.PointArg));
        if (BattleCheck(agent, t.PointArg))
        {
            return;
        }
        positionCmp.Position.SetValueAndForceNotify(t.PointArg);
        Dungeon.KeepPlayerInView();
    }
}
