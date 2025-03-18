using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguish.ECS.Systems;

internal class MovementSystem : IReactToEntitySystem
{
    public IGroup Group => new Group(typeof(PositionComponent), typeof(DisplayComponent));
    public IObservable<IEntity> ReactToEntity(IEntity entity)
    {
        var positionComponent = entity.GetComponent<PositionComponent>();
        var observable = (IObservable<Point>)positionComponent.Position;
        return observable.Select(_ => entity);
    }

    public void Process(IEntity entity)
    {
        var scEntity = entity.GetComponent<DisplayComponent>().ScEntity;
        var pos = entity.GetComponent<PositionComponent>().Position.Value;
        scEntity.Position = pos;
    }
}