using Roguish.ECS.Components;
using Roguish.Map_Generation;
using System.Diagnostics;
using EcsRx.Extensions;
using Roguish.ECS.Systems;
using Roguish.Info;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    private static bool BattleCheck(Point ptDest)
    {
        if (!MapGenerator.IsAgentAt(ptDest) || ptDest == EcsApp.PlayerPos)
        {
            // Takes two to tango...
            return false;
        }
        // Stop for any battles
        KeyboardEventSystem.StopQueue();

        var enemy = Mapgen.GetEntitiesAt(ptDest)[0];
        // TODO: MUCH more complicated battle algorithm here!
        var player = EcsRxApp.Player;
        var equippedCmp = player.GetComponent<EquippedComponent>();
        var hitDamage = 0;
        if (equippedCmp.WeaponLeft == -1 && equippedCmp.WeaponRight == -1)
        {
            var weaponInfo = WeaponInfo.InfoFromType(WeaponType.Fists);
            var fistDamage = weaponInfo.Damage;
            hitDamage += GoRogue.DiceNotation.Dice.Roll(fistDamage);
        }

        WeaponInfo? weaponInfo1 = null;
        if (equippedCmp.WeaponLeft > 0)
        {
            // Left Hand
            var weapon1 = EcsApp.EntityDatabase.GetEntity(equippedCmp.WeaponLeft);
            var weaponType1 = weapon1.GetComponent<WeaponTypeComponent>().WeaponType;
            weaponInfo1 = WeaponInfo.InfoFromType(weaponType1);
            var damage1 = weaponInfo1.Damage;
            hitDamage += GoRogue.DiceNotation.Dice.Roll(damage1);
        }
        if (weaponInfo1 != null && weaponInfo1.Slot != EquipSlots.TwoHands && equippedCmp.WeaponRight > 0)
        {
            // Right Hand
            var weapon2 = EcsApp.EntityDatabase.GetEntity(equippedCmp.WeaponRight);
            var weaponType2 = weapon2.GetComponent<WeaponTypeComponent>().WeaponType;
            var weaponInfo2 = WeaponInfo.InfoFromType(weaponType2);
            var damage2 = weaponInfo2.Damage;
            hitDamage += GoRogue.DiceNotation.Dice.Roll(damage2);
        }
        var enemyHealthCmp = enemy.GetComponent<HealthComponent>();
        var newHealth = Math.Max(0, enemyHealthCmp.CurrentHealth.Value - hitDamage);
        var name = enemy.GetComponent<DescriptionComponent>().Name;
        Log.PrintProcessedString($"You hit the [c:r f:Yellow]{name}[c:undo] for [c:r f:Red]{hitDamage}[c:undo] points of damage!");
        enemyHealthCmp.CurrentHealth.SetValueAndForceNotify(newHealth);
        if (newHealth == 0)
        {
            Log.PrintProcessedString($"You killed the [c:r f:Yellow]{name}[c:undo]!");
        }
        return true;
    }

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
        Debug.Assert(MapGenerator.IsWalkable(t.PointArg));
        if (BattleCheck(t.PointArg))
        {
            return;
        }
        positionCmp.Position.SetValueAndForceNotify(t.PointArg);
        Dungeon.KeepPlayerInView();
    }
}
