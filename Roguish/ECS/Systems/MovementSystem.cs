using System.Diagnostics;
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
    public DungeonSurface Dungeon { get; } = dungeon;
    public IGroup Group => new Group(typeof(PositionComponent), typeof(DisplayComponent));

    public IObservable<EcsEntity> ReactToEntity(EcsEntity entity)
    {
        var positionComponent = entity.GetComponent<PositionComponent>();
        IObservable<Point> observable = positionComponent.Position;
        return observable.Select(_ => entity);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Process the given entity when it's position changes </summary>
    ///
    /// <remarks>   As of right now the code assumes there is a DisplayComponent attached to the entity
    ///             and that its isVisible is set correctly.  
    ///             Darrell Plank, 4/14/2025. </remarks>
    ///
    /// <param name="entity">   The entity. </param>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public void Process(EcsEntity entity)
    {
        if (entity.HasComponent<IsDestroyedComponent>())
        {
            return;
        }
        // TODO: Do we need to worry about items without a displaycomponent?
        var scEntity = entity.GetComponent<DisplayComponent>().ScEntity;
        if (scEntity == null)
        {
            Debug.Assert(false, "Moving item with no Display component");
        }
        var type = entity.GetComponent<EntityTypeComponent>().EcsType;
        var posOld = scEntity.Position;
        var posCmp = entity.GetComponent<PositionComponent>();
        var posNew = posCmp.Position.Value;
        if (posNew == Point.Zero)
        {
            // Uninitialized position - nothing can be placed at Point.Zero
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