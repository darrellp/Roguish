using EcsRx.Extensions;
using Roguish.ECS.Components;
using IObservableGroup = EcsRx.Groups.Observable.IObservableGroup;

namespace Roguish.Screens;
internal class InventorySurface : ScreenSurface
{
    private static List<InventoryItem> _inventorySlots = new List<InventoryItem>(GameSettings.InvHeight);

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

    private record InventoryItem(int id, string name);
}
