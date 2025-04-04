using EcsRx.Extensions;
using Roguish.ECS.Components;
using SadConsole.Input;
using IObservableGroup = EcsRx.Groups.Observable.IObservableGroup;

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    private static List<InventoryItem> _inventorySlots = new List<InventoryItem>(GameSettings.InvHeight);
    private static int _selectedIndex = -1;

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

    private record InventoryItem(int id, string name);
}
