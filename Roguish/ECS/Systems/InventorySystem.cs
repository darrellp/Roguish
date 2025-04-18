using EcsRx.Groups;
using EcsRx.Systems;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Screens;

namespace Roguish.ECS.Systems;
internal class InventorySystem : ISetupSystem, ITeardownSystem
{
    private static readonly InventorySurface Inv;
    static InventorySystem()
    {
        Inv = Kernel.Get<InventorySurface>();
    }

    public IGroup Group => new Group(typeof(InBackpackComponent));
    public void Teardown(EcsEntity entity)
    {
        Inv.RemoveItem(entity.Id);
    }

    public void Setup(EcsEntity entity)
    {
        Inv.AddItem(entity.Id);
    }
}
