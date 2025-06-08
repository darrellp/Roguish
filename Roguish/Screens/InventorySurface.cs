using System;
using System.Diagnostics;
using EcsRx.Extensions;
using Roguish.ECS;
using Roguish.ECS.Components;
using SadConsole;
using SadConsole.Input;
// ReSharper disable IdentifierTypo

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    #region private fields
    private static List<InventoryItemNew> _inventorySlotsNew = new(GameSettings.InvHeight);
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
    private void AddItem(string name)
    {
        var iSlot = _inventorySlotsNew.FindIndex(item => item.Name == name);
        if (iSlot <= 0)
        {
            _inventorySlotsNew.Add(new InventoryItemNew(name, 1));
        }
        else
        {
            var isNew = _inventorySlotsNew[iSlot];
            _inventorySlotsNew[iSlot] = new InventoryItemNew(name, isNew.Count + 1);
        }
    }
    
    internal void RemoveItem(string name)
    {
        var index = _inventorySlotsNew.FindIndex(item => item.Name == name);

        // If we're trying to remove it then it better be in here!
        Debug.Assert(index >= 0);

        var slotSelected = _inventorySlotsNew[index];
        var count = slotSelected.Count;
        var newSlot = new InventoryItemNew(slotSelected.Name, count - 1);
        Monitor.Enter(_lock);
        if (count == 1)
        {
            _inventorySlotsNew.RemoveAt(index);

            for (var i = index; i < _inventorySlotsNew.Count; i++)
            {
                Surface.Print(0, i, _inventorySlotsNew[i].ToString(), Color.White);
            }

            return;
        }

        Surface.Print(0, index, newSlot.ToString());
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
        _inventorySlotsNew.Clear();
        Surface.Clear();
        _selectedIndex = -1;
    }
    #endregion

    #region Handlers
    protected override void OnMouseLeftClicked(MouseScreenObjectState state)
    {
        var (x, y) = state.CellPosition;
        if (y >= _inventorySlotsNew.Count)
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
            var slot = _inventorySlotsNew[_selectedIndex];
            Surface.Print(0, _selectedIndex, slot.ToString(), Color.White);
        }
        _selectedIndex = index;
        Surface.Print(0, index, _inventorySlotsNew[index].ToString(), Color.Orange);
    }

    internal static string SelectedEntity()
    {
        var user = EcsRxApp.Player;
        Debug.Assert(user.HasComponent<BackpackComponent>());
        var bp = user.GetComponent<BackpackComponent>();
        
        return _selectedIndex < 0 ? null : _inventorySlotsNew[_selectedIndex].Name;
    }
    #endregion


    internal void Equip()
    {
        var itemName = SelectedEntity();
        if (itemName == null)
        {
            _log.PrintProcessedString("No inventory items selected to equip");
            return;
        }
        var item = EcsRxApp.Player.GetComponent<BackpackComponent>().EntityFromName(itemName);
        Equip(item, EcsRxApp.Player);
    }

    internal static void Equip(EcsEntity item, EcsEntity agent)
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

    private record InventoryItemNew(string Name, int Count)
    {
        public override string ToString()
        {
            return Count > 1 ? $"{Name}({Count})" : Name;
        }
    };
}
