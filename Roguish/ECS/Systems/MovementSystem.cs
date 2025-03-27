using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;

namespace Roguish.ECS.Systems;

internal class MovementSystem(DungeonSurface dungeon) : IReactToEntitySystem
{
    private DungeonSurface _dungeon = dungeon;

    public IGroup Group => new Group(typeof(PositionComponent), typeof(DisplayComponent));

    public IObservable<EcsEntity> ReactToEntity(EcsEntity entity)
    {
        var positionComponent = entity.GetComponent<PositionComponent>();
        var observable = (IObservable<Point>)positionComponent.Position;
        return observable.Select(_ => entity);
    }

    public void Process(EcsEntity entity)
    {
        var scEntity = entity.GetComponent<DisplayComponent>().ScEntity;
        var posCmp = entity.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        if (pos == new Point())
        {
            // Uninitialized position
            return;
        }
        scEntity.Position = pos;
        if (Fov == null)
        {
            return;
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (entity.HasComponent(typeof(IsPlayerControlledComponent)))
        {
            Fov.Calculate(pos, GameSettings.FovRadius);
            DungeonSurface.SignalNewFov(posCmp.FDrawFullFov);
            posCmp.FDrawFullFov = false;
        }
        else if (_dungeon.DrawFov && entity.HasComponent(typeof(DisplayComponent)))
        {
            var playerDelta = pos - EcsApp.PlayerPos;
            var deltaModulus = playerDelta.X * playerDelta.X + playerDelta.Y * playerDelta.Y;
            if (deltaModulus > GameSettings.FovRadius * GameSettings.FovRadius)
            {
                entity.GetComponent<DisplayComponent>().ScEntity.IsVisible = false;
            }
            else
            {
                entity.GetComponent<DisplayComponent>().ScEntity.IsVisible = Fov.CurrentFOV.Contains(pos);
            }
        }
    }
}