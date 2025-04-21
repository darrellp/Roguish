using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.Info;
using System.Diagnostics;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    private static bool BattleCheck(EcsEntity attacker, Point ptDest)
    {
        var defender = Mapgen.GetAgentAt(ptDest);
        if (defender == null)
        {
            // Takes two to tango...
            return false;
        }

        var attackerIsPlayer = attacker.HasComponent<IsPlayerControlledComponent>();
        var defenderIsPlayer = defender.HasComponent<IsPlayerControlledComponent>();

        // One of them has to be the user (right now - may have monster to monster battles in the future)
        if (!attackerIsPlayer && !defenderIsPlayer)
        {
            return false;
        }
        Debug.Assert(!(attackerIsPlayer && defenderIsPlayer));

        // TODO: MUCH more complicated battle algorithm here!
        var defenderHealthCmp = defender.GetComponent<HealthComponent>();
        var attackerName = attacker.GetComponent<DescriptionComponent>().Name;
        var defenderName = defender.GetComponent<DescriptionComponent>().Name;
        var equippedCmpDefender = Utility.GetOrDefault<EquippedComponent>(defender);
        var damage = attackerIsPlayer ? PlayerAttackDamage(attacker) : Math.Max(0, 3 - equippedCmpDefender!.ArmorCount);
        var newHealth = Math.Max(0, defenderHealthCmp.CurrentHealth.Value - damage);
        var hitMsg = defenderIsPlayer
            ? $"The [c:r f:Yellow]{attackerName}[c:undo] hits you for [c:r f:orange]{damage}[c:undo] damage!"
            : $"You hit the [c:r f:Yellow]{defenderName}[c:undo] for [c:r f:Red]{damage}[c:undo] damage!";
        Log.PrintProcessedString(hitMsg);
        if (newHealth == 0)
        {
            if (defenderIsPlayer)
            {
                Log.PrintProcessedString("[c:r f:Red]*** Y O U   D I E D ! ! ! ***");
                Log.PrintProcessedString("[c:r f:Red]But you rise to life like a phoenix!");
                newHealth = 20;
            }
            else
            {
                Log.PrintProcessedString($"You killed the [c:r f:Yellow]{defenderName}[c:undo]!");
            }
        }
        defenderHealthCmp.CurrentHealth.SetValueAndForceNotify(newHealth);
        return true;
    }

    private static int PlayerAttackDamage(EcsEntity player)
    {
        var equippedCmp = player.GetComponent<EquippedComponent>();
        var hitDamage = 0;
        if (equippedCmp.WeaponLeft == -1 && equippedCmp.WeaponRight == -1)
        {
            var weaponInfo = WeaponInfo.InfoFromType(WeaponType.Fists);
            var fistDamage = weaponInfo.Damage;
            hitDamage += GoRogue.DiceNotation.Dice.Roll(fistDamage);
        }

        WeaponInfo? weaponInfoLeft = null;
        if (equippedCmp.WeaponLeft > 0)
        {
            // Left Hand
            var weapon1 = EcsApp.EntityDatabase.GetEntity(equippedCmp.WeaponLeft);
            var weaponType1 = weapon1.GetComponent<WeaponTypeComponent>().WeaponType;
            weaponInfoLeft = WeaponInfo.InfoFromType(weaponType1);
            var damage1 = weaponInfoLeft.Damage;
            hitDamage += GoRogue.DiceNotation.Dice.Roll(damage1);
        }
        if (weaponInfoLeft != null && weaponInfoLeft.Slot != EquipSlots.TwoHands && equippedCmp.WeaponRight > 0)
        {
            // Right Hand
            var weapon2 = EcsApp.EntityDatabase.GetEntity(equippedCmp.WeaponRight);
            var weaponType2 = weapon2.GetComponent<WeaponTypeComponent>().WeaponType;
            var weaponInfo2 = WeaponInfo.InfoFromType(weaponType2);
            var damage2 = weaponInfo2.Damage;
            hitDamage += GoRogue.DiceNotation.Dice.Roll(damage2);
        }
        return hitDamage;
    }

}