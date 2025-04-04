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
}
internal class EquipableComponent(EquipSlots equipSlot) : EcsComponent
{
    internal EquipSlots EquipSlot = equipSlot;
}