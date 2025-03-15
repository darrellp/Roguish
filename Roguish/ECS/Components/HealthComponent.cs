using EcsRx.Components;
using SystemsRx.ReactiveData;

namespace Roguish.ECS.Components;

internal class HealthComponent(int maxHealth) : IComponent, IDisposable
{
    public ReactiveProperty<int> CurrentHealth { get; set; } = new(maxHealth);
    public int MaxHealth { get; set; } = maxHealth;

    public void Dispose()
    {
        CurrentHealth.Dispose();
    }
}