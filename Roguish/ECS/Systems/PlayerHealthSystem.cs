using System.Reactive.Linq;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Systems;
using Roguish.ECS.Components;
using Roguish.Screens;

namespace Roguish.ECS.Systems;
internal class PlayerHealthSystem : IReactToEntitySystem
{
    public IGroup Group => new Group(typeof(HealthComponent), typeof(IsPlayerControlledComponent));
    public IObservable<EcsEntity> ReactToEntity(EcsEntity entity)
    {
        var healthComponent = entity.GetComponent<HealthComponent>();
        var observable = (IObservable<int>)healthComponent.CurrentHealth;
        return observable.Select(_ => entity);
    }

    public void Process(EcsEntity entity)
    {
        StatusBar.PlayerHealth.SetValueAndForceNotify(entity.GetComponent<HealthComponent>().CurrentHealth.Value);
    }
}
