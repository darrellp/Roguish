namespace Roguish.ECS.Components;

// Marks items that need to be deleted or attended to on a level change
public class LevelItemComponent(int level) : EcsComponent
{
    public int Level { get; set; } = level;
}