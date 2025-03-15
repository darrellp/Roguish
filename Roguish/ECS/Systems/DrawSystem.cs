using System.Reactive.Linq;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Ninject;
using Roguish.ECS.Components;

namespace Roguish.ECS.Systems;

internal class DrawSystem : IReactToEntitySystem
{
    public IGroup Group => new Group(typeof(DisplayComponent), typeof(PositionComponent));
    private readonly DungeonSurface _dungeonSurface;

    public DrawSystem()
    {
        _dungeonSurface = Program.Kernel.Get<DungeonSurface>();
    }

    public IObservable<IEntity> ReactToEntity(IEntity entity)
    {
        var positionComponent = entity.GetComponent<PositionComponent>();
        IObservable<Point> observable = positionComponent.Position;
        return observable.Select(_ => entity);
    }

    private void DrawEntity(IEntity entity)
    {
        var positionComponent = entity.GetComponent<PositionComponent>();
        var displayComponent = entity.GetComponent<DisplayComponent>();
        var position = positionComponent.Position.Value;
        var scEntity = displayComponent.ScEntity;
        //_dungeonSurface.DrawOvlyGlyph(glyph, position);
    }

    public void Process(IEntity entity)
    {
        DrawEntity(entity);
    }
}