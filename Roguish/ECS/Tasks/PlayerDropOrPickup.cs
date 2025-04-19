using System.Diagnostics;
using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Screens;

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    private static void OnChoosePickup(EcsEntity entity, List<int> selection)
    {
        if (selection.Count == 0)
        {
            return;
        }
        var posCmp = EcsRxApp.Player.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        var allIds = Mapgen.GetEntitiesAt(pos, true).Select(e=>e.Id).ToList();
        var ids = selection.Select(i => allIds[i]).ToList();
        var pickupCmp = new SelectedIdsComponent(ids);
        entity.AddComponent(pickupCmp);

        var taskCmp = entity.GetComponent<TaskComponent>();
        taskCmp.Tasks[0] = CreatePickupTask();
        EcsApp.EventSystem.Publish(new NewTurnEvent());
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   User pickup. </summary>
    ///
    /// <remarks>   This can be called twice times in the execution of a single task.  The first time is
    ///             when a normal pickup task is executed.  If there is only one item at the current player
    ///             position, it is picked up and we're done.  If there are multiple items, a dialog is
    ///             brought up to allow the user to select one or more items. The time to execute the dialog
    ///             is adjusted to be zero (i.e., the FireOn time is set to the current time).  This ensures
    ///             that no other tasks fire while the dialog is up.  When the dialog finishes up the
    ///             OnChoose method is called which sets the PickupComponent on the entity and sets the FireOn
    ///             time to the current time + the time to pick up the items.  It prepares a second pickup
    ///             call and prepares another UserPickup task which will be the second time this method
    ///             is called.  When it sees that PickupComponent is set, it will pick up the items in
    ///             that component and return.  Is there a better way to achieve this?  Maybe.  If
    ///             SadConsole had true Modal dialogs which blocked until they returned their info that
    ///             would be by far the preferable solution but they don't so we need someway to return to 
    ///             task processing while ensuring that no tasks actually get processed while the dialog
    ///             is up.  That's essentially what this does.  Another possibility would be to add arguments
    ///             to tasks but this seems problematic in terms of serialization and some other stuff.
    ///             I may change to that sometime though if it seems like this is happening too often.
    ///             Darrell Plank, 4/17/2025. </remarks>
    ///
    /// <param name="agent">    The agent. </param>
    /// <param name="t">        A RogueTask to process. </param>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    internal static void UserPickup(EcsEntity agent, RogueTask t)
    {
        var posCmp = agent.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        if (agent.HasComponent<SelectedIdsComponent>())
        {
            var pickupCmp = agent.GetComponent<SelectedIdsComponent>();
            var ids = pickupCmp.Ids;

            foreach (var id in ids)
            {
                MoveToBackpack(EcsApp.EntityDatabase.GetEntity(id), pos);
            }
            agent.RemoveComponent<SelectedIdsComponent>();
            t.FireOn = Ticks + (ulong)(PickUpTime * ids.Count);
            return;
        }
        var entities = Mapgen.GetEntitiesAt(pos, true);
        if (entities.Count > 1)
        {
            var names = entities
                .Where(e => e.HasComponent<DescriptionComponent>())
                .Select(e => e.GetComponent<DescriptionComponent>().Name)
                .ToList();
            var chooseDlg = new ChooseDialog("Choose an Item", names, agent, OnChoosePickup, true);
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

        var name = Utility.GetColoredName(entity);
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

        var name = Utility.GetColoredName(item);
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
