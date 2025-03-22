using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;

namespace Roguish.ECS.Systems;

internal class MovementSystem : IReactToEntitySystem
{
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
        scEntity.Position = pos;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Fov == null || !entity.HasComponent(typeof(IsPlayerControlledComponent)))
        {
            return;
        }

        Fov.Calculate(pos, GameSettings.FovRadius);
        NewDungeonSystem.SignalNewFov();
        posCmp.FDrawFullFov = false;
    }
}