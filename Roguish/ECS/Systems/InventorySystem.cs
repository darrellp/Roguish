using EcsRx.Groups;
using EcsRx.Systems;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Screens;

namespace Roguish.ECS.Systems;
internal class InventorySystem : ISetupSystem, ITeardownSystem
{
    private static readonly InventorySurface _inv;
    static InventorySystem()
    {
        _inv = Kernel.Get<InventorySurface>();
    }

    public IGroup Group => new Group(typeof(InBackpackComponent));
    public void Teardown(EcsEntity entity)
    {
        _inv.RemoveItem(entity.Id);
    }

    public void Setup(EcsEntity entity)
    {
        _inv.AddItem(entity.Id);
    }
}
