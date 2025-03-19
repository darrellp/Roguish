using SystemsRx.ReactiveData;

namespace Roguish.ECS.Components;

// Used to track the health of an entity

internal class HealthComponent(int maxHealth) : EcsComponent, IDisposable
{
    public ReactiveProperty<int> CurrentHealth { get; set; } = new(maxHealth);
    public int MaxHealth { get; set; } = maxHealth;

    public void Dispose()
    {
        CurrentHealth.Dispose();
    }
}