namespace Roguish.ECS.Components;

// Used to flag enemies

internal class AgentComponent : EcsComponent
{
    public MonsterType MonsterType { get; set; }
    public ulong MoveTime { get; set; }

    public AgentComponent(MonsterType monsterType, ulong moveTime)
    {
        MonsterType = monsterType;
        MoveTime=moveTime;
    }
}
