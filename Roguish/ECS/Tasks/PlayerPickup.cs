using EcsRx.Extensions;
using Roguish.ECS.Components;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static void UserPickup(EcsEntity agent)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        // TODO: Handle fMore being true here
        var (entity, fMore) = Mapgen.GetEntityAt(pos, true);
        if (entity != null && !entity.HasComponent<AgentComponent>())
        {
            if (entity.HasComponent<DisplayComponent>())
            {
                var displayCmp = entity.GetComponent<DisplayComponent>();
                displayCmp.ScEntity.IsVisible = false;
                Dungeon.RemoveScEntity(displayCmp.ScEntity);
                Mapgen.RemoveItemAt(pos, entity.Id);
            }

            var name = Utility.GetName(entity);

            Log.PrintProcessedString($"Picked up {name}");
            entity.RemoveComponent<PositionComponent>();
            entity.AddComponent<InBackpackComponent>();
        }
        else
        {
            Log.Cursor.Print("There's nothing to be picked up here").NewLine();
        }

        if (!agent.HasComponent<IsPlayerControlledComponent>())
        {
            agent.AddComponent(CreateAgentMoveTask(taskCmp.FireOn));
        }
    }

    internal static TaskComponent CreatePickupTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }

        return new(currentTicks + PickUpTime, UserPickup);
    }
}
