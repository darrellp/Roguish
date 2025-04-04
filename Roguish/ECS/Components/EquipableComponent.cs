namespace Roguish.ECS.Components;

internal enum EquipSlots
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
internal class EquipableComponent(EquipSlots equipSlot) : EcsComponent
{
    internal EquipSlots EquipSlot = equipSlot;
}