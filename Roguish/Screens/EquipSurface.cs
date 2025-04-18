using EcsRx.Extensions;
using Roguish.ECS.Components;

namespace Roguish.Screens;
internal class EquipSurface : ScreenSurface
{
    private const int LeftColumnWidth = 12;
    public EquipSurface() : base(GameSettings.EquipWidth, GameSettings.EquipHeight)
    {
        Position = GameSettings.EquipPosition;
        var equippedCmp = new EquippedComponent();
        Update(equippedCmp);
    }
    public void Update(EquippedComponent equipped)
    {
        var equipSlots = new List<EquipSlotInfo>
        {
            new("Left Hand", x => x.WeaponLeft),
            new("Right Hand", x => x.WeaponRight),
            new("Left Ring", x => x.LRing),
            new("Right Ring", x => x.RRing),
            new("Chest", x => x.Chest),
            new("Headgear", x => x.Headgear),
            new("Footwear", x => x.Footwear),
            new("Legs", x => x.Legs),
            new("Amulet", x => x.Amulet),
            new("Gloves", x => x.Gloves),
            new("Belt", x => x.Belt),
            new("Arms", x => x.Arms)
        };
        for (var i = 0; i < equipSlots.Count; i++)
        {
            var slot = equipSlots[i];
            var id = slot.Getter(equipped);
            var name = slot.Name.PadRight(LeftColumnWidth, '.');
            var value = id == -1 ? 
                "None" : 
                EcsApp.EntityDatabase.GetEntity(id).GetComponent<DescriptionComponent>().Name;
            Surface.Print(0, i, name, Color.AnsiCyanBright);
            Surface.Print(LeftColumnWidth, i, value.PadRight(GameSettings.EquipWidth - LeftColumnWidth), Color.Yellow);
        }
    }

    private record EquipSlotInfo(string Name, Func<EquippedComponent, int> Getter);
}
