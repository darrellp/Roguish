namespace Roguish.ECS.Components;

// Dunno how much of this I'll actually use but doesn't hurt to put it here
// TODO: Implement another of these
internal enum EcsType
{
    Player,
    Agent,
    Weapon,
    Potion,
    Inert,      // Statues and other blocking, unmoving stuff
    Door,
    Stairs,
    Trap,
    Armor,
    Scroll,
    Food,
    Gold,
    Corpse,
    Key,
    Container,
    LightSource,
    Projectile,
}

internal class EntityTypeComponent(EcsType type) : EcsComponent
{
    internal EcsType EcsType { get; init; } = type;
}
