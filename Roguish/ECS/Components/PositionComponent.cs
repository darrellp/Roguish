using EcsRx.Components;
using SystemsRx.ReactiveData;

namespace Roguish.ECS.Components;

internal class PositionComponent(Point position) : IComponent
{
    public ReactiveProperty<Point> Position { get; set; } = new(position);

    public PositionComponent() : this(new Point(0, 0)) {}

    public void Dispose()
    {
        Position.Dispose();
    }

}