namespace Roguish.ECS.Components;

// Marks items that need to be deleted or attended to on a level change
public class LevelItemComponent : EcsComponent
{
    public int Level { get; set; }

    public LevelItemComponent(int level)
    {
        Level = level;
    }
}