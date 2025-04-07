using Ninject;
using Roguish.ECS.Components;
using Roguish.Screens;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static RogueTask CreateTakeStairsTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks, TakeStairs);
    }

    public static void TakeStairs(EcsEntity agent, RogueTask t)
    {
        Dungeon.FillSurface(Kernel.Get<DungeonSurface>());
        Dungeon.IsFocused = true;
    }
}