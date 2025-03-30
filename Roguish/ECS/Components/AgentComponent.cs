namespace Roguish.ECS.Components;

// Used to flag enemies

internal class AgentComponent : EcsComponent
{
    public AgentType AgentType { get; set; }
    public ulong MoveTime { get; set; }

    public AgentComponent(AgentType agentType, ulong moveTime)
    {
        AgentType = agentType;
        MoveTime=moveTime;
    }
}
