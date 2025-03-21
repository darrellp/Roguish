namespace Roguish.ECS.Components;
internal class CellFovComponent(bool isInFov, Point cellPos) : EcsComponent
{
    public bool IsInFov => isInFov;
    public Point CellPos => cellPos;
}
