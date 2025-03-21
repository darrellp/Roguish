﻿using System.Reactive.Linq;
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
        var pos = entity.GetComponent<PositionComponent>().Position.Value;
        scEntity.Position = pos;

        if (Fov != null && entity.HasComponent(typeof(IsPlayerControlledComponent)))
        {
            Fov.Calculate(pos, GameSettings.FovRadius);
            FovSystem.SignalNewFov();
        }
    }
}