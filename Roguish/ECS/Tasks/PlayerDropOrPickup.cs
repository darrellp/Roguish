using System.Diagnostics;
using Coroutine;
using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.Screens;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    private static void OnChoose(List<int> selection)
    {
        var posCmp = EcsRxApp.Player.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        var entities = Mapgen.GetEntitiesAt(pos, true);
        foreach (var iSelected in selection)
        {
            MoveToBackpack(entities[iSelected], pos);
        }
    }

    internal static void UserPickup(EcsEntity agent, RogueTask t)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        var entities = Mapgen.GetEntitiesAt(pos, true);
        if (entities.Count > 1)
        {
            var names = entities
                .Where(e => e.HasComponent<DescriptionComponent>())
                .Select(e => e.GetComponent<DescriptionComponent>().Name)
                .ToList();
            var chooseDlg = new ChooseDialog("Choose an Item", names, OnChoose, true);
            chooseDlg.ShowDialog();
            // Don't fire any other tasks while the dialog is up
            t.FireOn = Ticks;
            return;
        }

        if (entities.Count != 0)
        {
            MoveToBackpack(entities[0], pos);
        }
        else
        {
            Log.Cursor.Print("There's nothing to be picked up here").NewLine();
        }
    }

    internal static void MoveToBackpack(EcsEntity entity, Point pos)
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

    internal static void UserDrop(EcsEntity agent, RogueTask t)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        var items = Mapgen.GetEntitiesAt(pos, true);
        if (items.Count >= 1 && items[0].HasComponent<StairsComponent>())
        {
            Debug.Assert(items.Count == 1);
            Log.PrintProcessedString("You can't drop items on top of stairs");
            return;
        }

        var itemPosCmp = new PositionComponent(pos);
        var item = InventorySurface.SelectedEntity();
        if (item == null)
        {
            Log.PrintProcessedString("Select an item to drop first");
            return;
        }

        // Take it off the inventory screen
        Inv.RemoveItem(item.Id);
        item.RemoveComponent<InBackpackComponent>();

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
