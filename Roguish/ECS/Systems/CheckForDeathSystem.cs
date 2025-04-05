using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Map_Generation;

namespace Roguish.ECS.Systems;

internal class CheckForDeathSystem : IReactToEntitySystem
{

    public IGroup Group => new Group(typeof(HealthComponent));

    public IObservable<EcsEntity> ReactToEntity(EcsEntity entity)
    {
        var healthComponent = entity.GetComponent<HealthComponent>();
        var observable = (IObservable<int>)healthComponent.CurrentHealth;
        return observable.Where(h => h <= 0).Select(_ => entity);
    }

    public void Process(EcsEntity entity)
    {
        var posCmp = entity.GetComponent<PositionComponent>();
        Kernel.Get<MapGenerator>().RemoveAgentAt(posCmp.Position.Value);
        entity.RemoveComponent<PositionComponent>();
        entity.AddComponent<IsDestroyedComponent>();
    }
}