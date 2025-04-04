using System.Diagnostics;
using EcsRx.Extensions;
using Ninject;
using Roguish.ECS;
using Roguish.ECS.Components;
using SadConsole.Input;
using IObservableGroup = EcsRx.Groups.Observable.IObservableGroup;

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    private static List<InventoryItem> _inventorySlots = new List<InventoryItem>(GameSettings.InvHeight);
    private static int _selectedIndex = -1;
    private static LogScreen _log;

    static InventorySurface()
    {
        _log = Kernel.Get<LogScreen>();
    }
    public InventorySurface() : base(GameSettings.InvWidth, GameSettings.InvHeight)
    {
        Position = GameSettings.InvPosition;
    }

    internal void AddItem(int id)
    {
        var entity = EcsApp.EntityDatabase.GetEntity(id);
        var name = entity.HasComponent<DescriptionComponent>()
            ? entity.GetComponent<DescriptionComponent>().Name
            : "Unnamed Object";
        Surface.Print(0, _inventorySlots.Count, name);
        _inventorySlots.Add(new InventoryItem(id, name));
    }

    protected override void OnMouseLeftClicked(MouseScreenObjectState state)
    {
        var (x, y) = state.CellPosition;
        var index = y;
        if (index >= _inventorySlots.Count)
            return;
        MoveHighlightTo(index);
    }

    private void MoveHighlightTo(int index)
    {
        if (index == _selectedIndex)
        {
            return;
        }

        if (_selectedIndex >= 0)
        {
            var name = _inventorySlots[_selectedIndex].name;
            Surface.Print(0, _selectedIndex, name, Color.White);
        }
        _selectedIndex = index;
        Surface.Print(0, index, _inventorySlots[index].name, Color.Orange);
    }

    internal void Equip()
    {
        var item = EcsApp.EntityDatabase.GetEntity(_inventorySlots[_selectedIndex].id);
        Debug.Assert(item != null);
        EquipableComponent? equipableCmp;
        if (!item.HasComponent<EquipableComponent>())
        {
            var name = item.HasComponent<DescriptionComponent>()
                ? Utility.PrefixWithAorAnColored(item.GetComponent<DescriptionComponent>().Name, "Yellow")
                : "an [c:r f:Yellow]unnamed object";
            _log.PrintProcessedString($"We can't equip {name}");
            return;
        }

        equipableCmp = item.GetComponent<EquipableComponent>();
        item.RemoveComponent<InBackpackComponent>();
        var equippedCmp = EcsRxApp.Player.GetComponent<EquippedComponent>();
        var id = item.Id;
        var oldId = -1;
        var oldIdAlt = -1;

        switch (equipableCmp.EquipSlot)
        {
            case EquipSlots.Amulet:
                oldId = equippedCmp.Amulet;
                equippedCmp.Amulet = id;
                break;

            case EquipSlots.Footwear:
                oldId = equippedCmp.Footwear;
                equippedCmp.Footwear = id;
                break;

            case EquipSlots.Headgear:
                oldId = equippedCmp.Headgear;
                equippedCmp.Headgear = id;
                break;

            case EquipSlots.Legs:
                oldId = equippedCmp.Legs;
                equippedCmp.Legs = id;
                break;

            case EquipSlots.Chest:
                oldId = equippedCmp.Chest;
                equippedCmp.Chest = id;
                break;

            case EquipSlots.OneHand:
                oldId = equippedCmp.WeaponLeft;
                if (oldId == -1)
                {
                    oldIdAlt = equippedCmp.WeaponRight;
                    equippedCmp.WeaponRight = id;
                }
                else
                {
                    equippedCmp.WeaponLeft = id;
                }
                break;

            case EquipSlots.Ring:
                oldId = equippedCmp.LRing;
                if (oldId == -1)
                {
                    oldIdAlt = equippedCmp.RRing;
                    equippedCmp.RRing = id;
                }
                else
                {
                    equippedCmp.LRing = id;
                }
                break;

            case EquipSlots.TwoHands:
                oldId = equippedCmp.WeaponLeft;
                oldIdAlt = equippedCmp.WeaponRight;
                equippedCmp.WeaponLeft = equippedCmp.WeaponRight = id;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (oldId >= 0)
        {
            var replaced = EcsApp.EntityDatabase.GetEntity(oldId);
            Debug.Assert(replaced != null);
            replaced.AddComponent<InBackpackComponent>();
            AddItem(replaced.Id);
        }
        if (oldIdAlt >= 0)
        {
            var replaced = EcsApp.EntityDatabase.GetEntity(oldIdAlt);
            Debug.Assert(replaced != null);
            replaced.AddComponent<InBackpackComponent>();
            AddItem(replaced.Id);
        }
        // TODO: keyboard hotkey, Equipped screen, locking _inventorySlots because we want to both add and remove
    }

    private record InventoryItem(int id, string name);
}
