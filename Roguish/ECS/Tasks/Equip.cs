using Roguish.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static TaskComponent CreateEquipTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + EquipTime, UserEquip);
    }

    public static void UserEquip(EcsEntity agent, RogueTask t)
    {
        Inv.Equip();
    }
}
