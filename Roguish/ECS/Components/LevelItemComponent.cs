namespace Roguish.ECS.Components;

// Marks items that need to be deleted or attended to on a level change
internal class LevelItemComponent : EcsComponent
{
    public int Level { get; set; }

    internal LevelItemComponent(int level)
    {
        Level = level;
    }
}