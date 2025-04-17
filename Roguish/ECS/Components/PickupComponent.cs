namespace Roguish.ECS.Components;
internal class PickupComponent : EcsComponent
{
    internal List<int> Ids { get; set; } = new List<int>();
    public PickupComponent(List<int> ids)
    {
        Ids = ids;
    }
}
