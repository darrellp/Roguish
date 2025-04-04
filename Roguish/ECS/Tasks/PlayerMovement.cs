using Roguish.ECS.Components;
using Roguish.Map_Generation;
using System.Diagnostics;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
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
        Dungeon.KeepPlayerInView();
    }

    internal static TaskComponent CreatePlayerMoveTask(Point newPt, ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + StdMovementTime, MovePlayerClosure(newPt));
    }
}
