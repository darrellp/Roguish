using Roguish.ECS.Components;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static RogueTask CreateEquipTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + EquipTime, TaskType.Equip);
    }

    public static void UserEquip(EcsEntity agent, RogueTask t)
    {
        Inv.Equip();
    }
}
