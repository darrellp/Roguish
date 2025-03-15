using SystemsRx.ReactiveData;

namespace Roguish.ECS.Components;

internal class PositionComponent(Point position) : EcsComponent, IDisposable
{
    public ReactiveProperty<Point> Position { get; set; } = new(position);

    public PositionComponent() : this(new Point(0, 0)) {}

    public void Dispose()
    {
        Position.Dispose();
    }

}