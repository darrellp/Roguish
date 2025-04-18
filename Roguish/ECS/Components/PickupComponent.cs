namespace Roguish.ECS.Components;
internal class PickupComponent(List<int> ids) : EcsComponent
{
    internal List<int> Ids { get; set; } = ids;
}
