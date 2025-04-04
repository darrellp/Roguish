using Roguish.ECS.Components;
using Roguish.Map_Generation;
using System.Diagnostics;
using EcsRx.Extensions;

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
        if (BattleCheck(newPosition))
        {
            return;
        }
        positionCmp.Position.SetValueAndForceNotify(newPosition);
        Dungeon.KeepPlayerInView();
    }

    private static bool BattleCheck(Point ptDest)
    {
        if (!Mapgen.IsAgentAt(ptDest))
        {
            // Takes two to tango...
            return false;
        }
        var (enemy,_) = Mapgen.GetEntityAt(ptDest);
        // TODO: MUCH more complicated battle algorithm here!
        var enemyHealthCmp = enemy.GetComponent<HealthComponent>();
        var newHealth = Math.Max(0, enemyHealthCmp.CurrentHealth.Value - 5);
        enemyHealthCmp.CurrentHealth.SetValueAndForceNotify(newHealth);
        return true;
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
