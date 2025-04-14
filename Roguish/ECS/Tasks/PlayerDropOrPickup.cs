using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.Screens;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    internal static void UserPickup(EcsEntity agent, RogueTask t)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;
        // TODO: Handle fMore being true here
        var (entity, fMore) = Mapgen.GetEntityAt(pos, true);
        if (entity != null && !entity.HasComponent<AgentTypeComponent>())
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
    }

    internal static void UserDrop(EcsEntity agent, RogueTask t)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var taskCmp = agent.GetComponent<TaskComponent>();
        var pos = posCmp.Position.Value;

        var itemPosCmp = new PositionComponent(pos);
        var item = InventorySurface.SelectedEntity();
        if (item == null)
        {
            Log.PrintProcessedString("Select an item to drop first");
            return;
        }

        // Take it off the inventory screen
        Inv.RemoveItem(item.Id);

        if (item.HasComponent<DisplayComponent>())
        {
            var scEntity = item.GetComponent<DisplayComponent>().ScEntity;
            Dungeon.AddScEntity(scEntity);
        }
        item.AddComponent(itemPosCmp);

        var name = Utility.GetName(item);
        Log.PrintProcessedString($"Dropped {name}");
    }

    internal static RogueTask CreatePickupTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }

        return new(currentTicks + PickUpTime, TaskType.PickUp);
    }

    internal static RogueTask CreateDropTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + PickUpTime, TaskType.Drop);
    }
}
