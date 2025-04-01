using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;
using Roguish.Map_Generation;
using Roguish.Screens;

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
        if (entity.HasComponent<IsDestroyedComponent>())
        {
            return;
        }
        var scEntity = entity.GetComponent<DisplayComponent>().ScEntity;
        var type = entity.GetComponent<EntityTypeComponent>().EcsType;
        var posOld = scEntity.Position;
        var posCmp = entity.GetComponent<PositionComponent>();
        var posNew = posCmp.Position.Value;
        if (posNew == new Point())
        {
            // Uninitialized position
            return;
        }
        scEntity.Position = posNew;
        MapGenerator.SetAgentPosition(entity.Id, posOld, type, posNew);
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
            DetermineVisibility(scEntity);
        }
    }

    internal static void DetermineVisibility(ScEntity entity)
    {
        var playerDelta = entity.Position - EcsApp.PlayerPos;
        var deltaModulus = playerDelta.X * playerDelta.X + playerDelta.Y * playerDelta.Y;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (deltaModulus > GameSettings.FovRadius * GameSettings.FovRadius)
        {
            entity.IsVisible = false;
        }
        else
        {
            entity.IsVisible = Fov.CurrentFOV.Contains(entity.Position);
        }
    }
}