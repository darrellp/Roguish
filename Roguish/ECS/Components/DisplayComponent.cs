using SadConsole.Entities;
namespace Roguish.ECS.Components;

internal class DisplayComponent(Entity entity) : EcsComponent
{
    public Entity ScEntity{ get; set; } = entity;
}