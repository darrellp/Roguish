// ReSharper disable IdentifierTypo
namespace Roguish.ECS.Components;

public enum EquipSlots
{
    OneHand,
    TwoHands,
    Ring,
    Amulet,
    Headgear,
    Footwear,
    Legs,
    Chest,
    Gloves,
    Arms,
    Belt
}
public class EquipableComponent(EquipSlots equipSlot) : EcsComponent
{
    public EquipSlots EquipSlot { get; set; } = equipSlot;
}