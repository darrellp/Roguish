using Roguish.Info;

namespace Roguish.ECS.Components;
internal class ArmorTypeComponent(ArmorType armorType) : EcsComponent
{
    public ArmorType ArmorType { get; set; } = armorType;
}
