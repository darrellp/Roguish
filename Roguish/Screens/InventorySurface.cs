using System.Diagnostics;
using EcsRx.Extensions;
using Roguish.ECS;
using Roguish.ECS.Components;
using SadConsole.Input;
// ReSharper disable IdentifierTypo

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    private static List<InventoryItem> _inventorySlots = new(GameSettings.InvHeight);
    private static int _selectedIndex = -1;
    private static LogScreen _log = null!;
    private static EquipSurface _equip = null!;
    private static object _lock = new();
    private static string _clearLine = "".PadRight(GameSettings.InvWidth);

    public InventorySurface(EquipSurface equip, LogScreen log) : base(GameSettings.InvWidth, GameSettings.InvHeight)
    {
        Position = GameSettings.InvPosition;
        _log = log;
        _equip = equip;
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
        var index = _inventorySlots.FindIndex(item => item.Id == id);
        if (index < 0)
        {
            return;
        }
        Monitor.Enter(_lock);
        _inventorySlots.RemoveAt(index);
        for (var i = index; i < _inventorySlots.Count; i++)
        {
            var name = _inventorySlots[i].Name.PadRight(GameSettings.InvWidth);
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

    internal void Clear()
    {
        _inventorySlots.Clear();
        Surface.Clear();
        _selectedIndex = -1;
    }

    protected override void OnMouseLeftClicked(MouseScreenObjectState state)
    {
        var (x, y) = state.CellPosition;
        if (y >= _inventorySlots.Count)
            return;
        MoveHighlightTo(y);
    }

    private void MoveHighlightTo(int index)
    {
        if (index == _selectedIndex)
        {
            return;
        }

        if (_selectedIndex >= 0)
        {
            var name = _inventorySlots[_selectedIndex].Name;
            Surface.Print(0, _selectedIndex, name, Color.White);
        }
        _selectedIndex = index;
        Surface.Print(0, index, _inventorySlots[index].Name, Color.Orange);
    }

    internal static EcsEntity? SelectedEntity()
    {
        return _selectedIndex < 0 ? null : EcsApp.EntityDatabase.GetEntity(_inventorySlots[_selectedIndex].Id);
    }

    internal void Equip()
    {
        var item = SelectedEntity();
        if (item == null)
        {
            _log.PrintProcessedString("No inventory items selected to equip");
            return;
        }
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
        int oldId;
        var oldIdAlt = -1;

        item.RemoveComponent<InBackpackComponent>();
        item.AddComponent<IsEquippedComponent>();

        switch (equipableCmp.EquipSlot)
        {
            case EquipSlots.Gloves:
                oldId = equippedCmp.Gloves;
                equippedCmp.Gloves = id;
                break;

            case EquipSlots.Belt:
                oldId = equippedCmp.Belt;
                equippedCmp.Belt = id;
                break;

            case EquipSlots.Arms:
                oldId = equippedCmp.Arms;
                equippedCmp.Arms = id;
                break;

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
            replaced.RemoveComponent<IsEquippedComponent>();
        }
        if (oldIdAlt >= 0)
        {
            var replaced = EcsApp.EntityDatabase.GetEntity(oldIdAlt);
            Debug.Assert(replaced != null);
            replaced.AddComponent<InBackpackComponent>();
            replaced.RemoveComponent<IsEquippedComponent>();
        }
        _log.PrintProcessedString($"Equipped {Utility.GetName(item)}");
        _equip.Update(equippedCmp);
    }

    private record InventoryItem(int Id, string Name);
}
