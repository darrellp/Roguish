using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;
using Roguish.Map_Generation;

namespace Roguish.ECS.Systems;

internal class MovementSystem(DungeonSurface dungeon) : IReactToEntitySystem
{
    public IGroup Group => new Group(typeof(PositionComponent), typeof(DisplayComponent));

    public IObservable<EcsEntity> ReactToEntity(EcsEntity entity)
    {
        var positionComponent = entity.GetComponent<PositionComponent>();
        IObservable<Point> observable = positionComponent.Position;
        return observable.Select(_ => entity);
    }

    public void Process(EcsEntity entity)
    {
        var scEntity = entity.GetComponent<DisplayComponent>().ScEntity;
        var posOld = scEntity.Position;
        var posCmp = entity.GetComponent<PositionComponent>();
        var posNew = posCmp.Position.Value;
        if (posNew == new Point())
        {
            // Uninitialized position
            return;
        }
        scEntity.Position = posNew;
        MapGenerator.SetScEntityPosition(scEntity, posOld, posNew);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Fov == null)
        {
            return;
        }
        if (entity.HasComponent(typeof(IsPlayerControlledComponent)))
        {
            Fov.Calculate(posNew, GameSettings.FovRadius);
            DungeonSurface.SignalNewFov(posCmp.FDrawFullFov);
            posCmp.FDrawFullFov = false;
        }
        else if (dungeon.DrawFov && entity.HasComponent(typeof(DisplayComponent)))
        {
            var playerDelta = posNew - EcsApp.PlayerPos;
            var deltaModulus = playerDelta.X * playerDelta.X + playerDelta.Y * playerDelta.Y;
            if (deltaModulus > GameSettings.FovRadius * GameSettings.FovRadius)
            {
                entity.GetComponent<DisplayComponent>().ScEntity.IsVisible = false;
            }
            else
            {
                entity.GetComponent<DisplayComponent>().ScEntity.IsVisible = Fov.CurrentFOV.Contains(posNew);
            }
        }
    }
}