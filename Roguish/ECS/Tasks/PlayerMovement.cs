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
        if (!Mapgen.IsAgentAt(ptDest) || ptDest == EcsApp.PlayerPos)
        {
            // Takes two to tango...
            return false;
        }
        var (enemy,_) = Mapgen.GetEntityAt(ptDest);
        // TODO: MUCH more complicated battle algorithm here!
        var enemyHealthCmp = enemy.GetComponent<HealthComponent>();
        var newHealth = Math.Max(0, enemyHealthCmp.CurrentHealth.Value - 5);
        var name = enemy.GetComponent<DescriptionComponent>().Name;
        Log.PrintProcessedString($"You hit the [c:r f:Yellow]{name}[c:undo] for 5 points of damage!");
        enemyHealthCmp.CurrentHealth.SetValueAndForceNotify(newHealth);
        if (newHealth == 0)
        {
            Log.PrintProcessedString($"You killed the [c:r f:Yellow]{name}[c:undo]!");
        }
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
