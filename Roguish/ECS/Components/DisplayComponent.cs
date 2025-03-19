namespace Roguish.ECS.Components;

// Contains the ScEntity that is displayed for this entity
// 
internal class DisplayComponent(ScEntity scEntity) : EcsComponent
{
    public ScEntity ScEntity{ get; set; } = scEntity;
}