namespace Roguish.ECS.Components;
internal class WeaponInfoComponent(WeaponType weaponType) : EcsComponent
{
    public WeaponType WeaponType { get; set; } = weaponType;
}
