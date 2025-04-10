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
        return new(currentTicks, TaskType.TakeStairs);
    }

    public static void TakeStairs(EcsEntity agent, RogueTask t)
    {
        CurrentLevel++;
        Dungeon.FillSurface(Dungeon);
        Dungeon.IsFocused = true;
    }
}