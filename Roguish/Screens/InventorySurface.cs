using System.Diagnostics;
using EcsRx.Extensions;
using Ninject;
using Roguish.ECS;
using Roguish.ECS.Components;
using SadConsole.Input;
// ReSharper disable IdentifierTypo

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    private static List<InventoryItem> _inventorySlots = new List<InventoryItem>(GameSettings.InvHeight);
    private static int _selectedIndex = -1;
    private static LogScreen _log;
    private static object _lock = new object();
    private static string _clearLine = "".PadRight(GameSettings.InvWidth);

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
        Monitor.Enter(_lock);
        Surface.Print(0, _inventorySlots.Count, name, Color.White);
        _inventorySlots.Add(new InventoryItem(id, name));
        Monitor.Exit(_lock);
    }
    
    internal void RemoveItem(int id)
    {
        var index = _inventorySlots.FindIndex(item => item.id == id);
        if (index < 0)
            return;
        Monitor.Enter(_lock);
        _inventorySlots.RemoveAt(index);
        for (var i = index; i < _inventorySlots.Count; i++)
        {
            var name = _inventorySlots[i].name.PadRight(GameSettings.InvWidth);
            Surface.Print(0, i, name, Color.White);
        }

        Surface.Print(0, _inventorySlots.Count, _clearLine);
        if (_selectedIndex > index)
        {

            MoveHighlightTo(--_selectedIndex);
        }
        else if (_selectedIndex == index)
        {
            _selectedIndex = -1;
        }
        Monitor.Exit(_lock);
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
        if (_selectedIndex < 0)
        {
            _log.PrintProcessedString("No inventory items selected to equip");
            return;
        }

        var item = EcsApp.EntityDatabase.GetEntity(_inventorySlots[_selectedIndex].id);
        Debug.Assert(item != null);
        if (!item.HasComponent<EquipableComponent>())
        {
            var name = Utility.GetName(item);
            _log.PrintProcessedString($"We can't equip {name}");
            return;
        }

        var equipableCmp = item.GetComponent<EquipableComponent>();
        var equippedCmp = EcsRxApp.Player.GetComponent<EquippedComponent>();
        var id = item.Id;
        var oldId = -1;
        var oldIdAlt = -1;

        item.RemoveComponent<InBackpackComponent>();

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
                if (oldId >= 0)
                {
                    if (equippedCmp.WeaponRight == equippedCmp.WeaponLeft)
                    {
                        // Two handed weapon - remove it and place new in left
                        equippedCmp.WeaponLeft = id;
                        equippedCmp.WeaponRight = -1;
                    }
                    else
                    {
                        // Something in the left so let's put in the right
                        oldId = equippedCmp.WeaponRight;
                        equippedCmp.WeaponRight = id;
                    }
                }
                else
                {
                    equippedCmp.WeaponLeft = id;
                }
                break;

            case EquipSlots.Ring:
                oldId = equippedCmp.LRing;
                if (oldId >= 0)
                {
                    // Ring already on the left so use the right
                    oldId = equippedCmp.RRing;
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
        }
        if (oldIdAlt >= 0)
        {
            var replaced = EcsApp.EntityDatabase.GetEntity(oldIdAlt);
            Debug.Assert(replaced != null);
            replaced.AddComponent<InBackpackComponent>();
        }
    }

    private record InventoryItem(int id, string name);
}
