namespace Roguish.ECS.Components;
internal struct CellVisibility(bool visible) : EcsComponent
{
    public bool Visible { get; set; } = visible;
}
