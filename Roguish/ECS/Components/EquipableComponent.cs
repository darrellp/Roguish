namespace Roguish.ECS.Components;

internal enum EquipSlots
{
    OneHand,
    TwoHands,
    Ring,
    Amulet,
    Headgear,
    Footwear,
    Pants,
    Torso,
}
internal class EquipableComponent : EcsComponent
{
    internal EquipSlots EquipSlot;
}