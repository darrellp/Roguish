namespace Roguish.ECS.EcsEvents;
internal class LevelChangeEvent(int newLevel)
{
    public int NewLevel { get; set; } = newLevel;
}
