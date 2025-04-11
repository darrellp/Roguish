namespace Roguish.ECS.Components;
internal class WeaponTypeComponent(WeaponType weaponType) : EcsComponent
{
    public WeaponType WeaponType { get; set; } = weaponType;
}
