namespace Roguish.ECS.Components;

// Used to flag enemies

internal class AgentTypeComponent(AgentType agentType, ulong moveTime) : EcsComponent
{
    public AgentType AgentType { get; set; } = agentType;
    public ulong MoveTime { get; set; } = moveTime;
}
