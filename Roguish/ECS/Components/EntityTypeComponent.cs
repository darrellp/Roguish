namespace Roguish.ECS.Components;

// Dunno how much of this I'll actually use but doesn't hurt to put it here
// TODO: Implement another of these
public enum EcsType
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

public class EntityTypeComponent(EcsType type) : EcsComponent
{
    public EcsType EcsType { get; set; } = type;
}
