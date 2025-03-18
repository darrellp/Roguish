namespace Roguish.ECS.Components;

internal class DisplayComponent(ScEntity scEntity) : EcsComponent
{
    public ScEntity ScEntity{ get; set; } = scEntity;
}