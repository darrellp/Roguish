using Newtonsoft.Json;

namespace Roguish.ECS.Components;

// Contains the ScEntity that is displayed for this entity
// 
internal class DisplayComponent : EcsComponent
{
    public DisplayComponent(ScEntity scEntity)
    {
        ScEntity = scEntity;
    }

    public ScEntity ScEntity{ get; set; }
}