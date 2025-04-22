using System.Diagnostics;
using EcsRx.Extensions;
using Roguish.ECS;
using Roguish.ECS.Components;
using SadConsole.Input;
// ReSharper disable IdentifierTypo

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    #region private fields
    private static List<InventoryItem> _inventorySlots = new(GameSettings.InvHeight);
    private static int _selectedIndex = -1;
    private static LogScreen _log = null!;
    private static EquipSurface _equip = null!;
    private static object _lock = new();
    private static string _clearLine = "".PadRight(GameSettings.InvWidth);
    #endregion

    #region Constructor
    public InventorySurface(EquipSurface equip, LogScreen log) : base(GameSettings.InvWidth, GameSettings.InvHeight)
    {
        Position = GameSettings.InvPosition;
        _log = log;
        _equip = equip;
    }
    #endregion

    #region Equipping
    #region Adding/Removing
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
    #endregion

    #region Handlers
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
    #endregion


    internal void Equip()
    {
        var item = SelectedEntity();
        if (item == null)
        {
            _log.PrintProcessedString("No inventory items selected to equip");
            return;
        }
        Equip(item, EcsRxApp.Player);
    }

    internal void Equip(EcsEntity item, EcsEntity agent)
    {
        var isPlayer = agent == EcsRxApp.Player;
        if (!item.HasComponent<EquipableComponent>())
        {
            if (isPlayer)
            {
                var name = Utility.GetColoredName(item);
                _log.PrintProcessedString($"We can't equip {name}");
            }
            return;
        }

        var equipableCmp = item.GetComponent<EquipableComponent>();
        var equippedCmp = agent.GetComponent<EquippedComponent>();
        var oldIdAlt = -1;

        item.RemoveComponent<InBackpackComponent>();
        item.AddComponent<IsEquippedComponent>();

        var equipSlot = equipableCmp.EquipSlot;
        var slotInfo = equippedCmp.AvailableSlotFromSlotType(equipSlot);
        var oldId = slotInfo.Id;
        if (equipSlot == EquipSlots.TwoHands)
        {
            oldIdAlt = equippedCmp.WeaponRight;
        }
        else if (equipSlot == EquipSlots.OneHand && equippedCmp.WeaponLeft == equippedCmp.WeaponRight)
        {
            equippedCmp.WeaponRight = -1;
        }

        if (slotInfo.Id >= 0)
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

        slotInfo.SetId(item.Id);
        if (isPlayer)
        {
            _log.PrintProcessedString($"Equipped {Utility.GetColoredName(item)}");
            _equip.Update(equippedCmp);
        }
    }
    #endregion

    private record InventoryItem(int Id, string Name);
}
