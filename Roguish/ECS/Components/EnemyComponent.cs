namespace Roguish.ECS.Components;

// Used to flag enemies

internal class EnemyComponent : EcsComponent
{
    public MonsterType MonsterType { get; set; }

    public EnemyComponent(MonsterType monsterType)
    {
        MonsterType = monsterType;
    }
}
