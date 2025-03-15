namespace Roguish.ECS.Components;

internal class MoveComponent(Point move) : EcsComponent
{
    public Point MoveValue { get; set; } = move;

    public MoveComponent() : this(new Point(0, 0))
    {
        MoveValue = new Point(0, 0);
    }
}