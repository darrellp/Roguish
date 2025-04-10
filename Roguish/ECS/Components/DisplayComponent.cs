using Newtonsoft.Json;

namespace Roguish.ECS.Components;

// Contains the ScEntity that is displayed for this entity
// 
internal class DisplayComponent(ScEntity scEntity) : EcsComponent
{
    [JsonIgnore]
    public ScEntity ScEntity{ get; set; } = scEntity;
}