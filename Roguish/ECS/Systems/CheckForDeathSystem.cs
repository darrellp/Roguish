using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;

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
        entity.RemoveComponent<HealthComponent>();
        entity.AddComponent<IsDestroyedComponent>();
    }
}