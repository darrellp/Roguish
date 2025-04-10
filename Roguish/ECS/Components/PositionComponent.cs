using SystemsRx.ReactiveData;

namespace Roguish.ECS.Components;

// Position of the entity

public class PositionComponent(Point position, bool fDrawFullFov = false) : EcsComponent, IDisposable
{
    public ReactiveProperty<Point> Position { get; set; } = new(position);
    public bool FDrawFullFov = fDrawFullFov;

    public void Dispose()
    {
        Position.Dispose();
    }

}