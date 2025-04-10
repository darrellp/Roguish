using Roguish.ECS.Components;
using EcsRx.Extensions;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static RogueTask CreateRegenerateTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }

        return new RogueTask(currentTicks + EquipTime, TaskType.Regenerate);
    }

    internal static void Regenerate(EcsEntity agent, RogueTask t)
    {
        var healthCmp = agent.GetComponent<HealthComponent>();
        var health = healthCmp.CurrentHealth.Value;
        if (health < healthCmp.MaxHealth)
        {
            healthCmp.CurrentHealth.SetValueAndForceNotify(health + 1);
        }

        t.FireOn += 300;
    }
}