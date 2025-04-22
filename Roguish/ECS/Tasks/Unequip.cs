using System.Diagnostics;
using EcsRx.Extensions;
using Ninject;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Screens;
// ReSharper disable IdentifierTypo

namespace Roguish.ECS.Tasks;
internal partial class TaskGetter
{
    private static void OnChooseUnequip(EcsEntity entity, List<int> selection)
    {
        if (selection.Count == 0)
        {
            return;
        }
        var equipped = entity.GetComponent<EquippedComponent>();
        var slots = equipped.GetFilledSlots();
        var ids = selection.Select(i => slots[i].Id).ToList();
        var selectedCmp = new SelectedIdsComponent(ids);
        entity.AddComponent(selectedCmp);

        var taskCmp = entity.GetComponent<TaskComponent>();
        taskCmp.Tasks[0] = CreateUnequipTask();
        EcsApp.EventSystem.Publish(new NewTurnEvent());
    }

    internal static RogueTask CreateUnequipDialogTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + EquipTime, TaskType.UnequipDialog);
    }

    public static void UnequipDialog(EcsEntity agent, RogueTask t)
    {
        // Don't fire any other tasks while the dialog is up by setting our time to current time - i.e.,
        // make this a zero length task and no other tasks fire during a zero length task.
        t.FireOn = Ticks;

        var equipped = agent.GetComponent<EquippedComponent>();

        var slots = equipped.GetFilledSlots();
        if (slots.Count == 0)
        {
            Log.Cursor.Print("You have nothing equipped!").NewLine();
            return;
        }
        var names = slots.Select(slot => Utility.EntityName(slot.Id)).ToList();
        var chooseDlg = new ChooseDialog("Choose an Item", names, agent, OnChooseUnequip, true);
        chooseDlg.ShowDialog();
    }

    internal static RogueTask CreateUnequipTask(ulong currentTicks = ulong.MaxValue)
    {
        if (currentTicks == ulong.MaxValue)
        {
            currentTicks = Ticks;
        }
        return new(currentTicks + UnequipTime, TaskType.Unequip);
    }

    public static void Unequip(EcsEntity agent, RogueTask t)
    {
        Debug.Assert(agent.HasComponent<SelectedIdsComponent>(), "Somehow we didn't get a selected ids component from the chooser dialog");
        var selectedIds = agent.GetComponent<SelectedIdsComponent>();
        var equippedCmp = agent.GetComponent<EquippedComponent>();
        var equippedSlots = equippedCmp.GetFilledSlots();
        foreach (var slot in equippedSlots)
        {
            if (!selectedIds.Ids.Contains(slot.Id))
            {
                continue;
            }
            equippedCmp.UnequipSlot(slot);
            var entity = EcsApp.EntityDatabase.GetEntity(slot.Id);
            UnequipEntity(entity);
        }
        agent.RemoveComponent<SelectedIdsComponent>();
        Kernel.Get<EquipSurface>().Update(equippedCmp);
        t.FireOn = Ticks + (ulong)(UnequipTime * equippedSlots.Count);
    }

    internal static void UnequipEntity(EcsEntity entity)
    {
        var name = Utility.GetColoredName(entity);
        Log.PrintProcessedString($"Unequipped {name}");
        entity.RemoveComponent<IsEquippedComponent>();
        entity.AddComponent<InBackpackComponent>();
    }

}