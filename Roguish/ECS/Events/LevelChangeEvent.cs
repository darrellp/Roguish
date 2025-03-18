namespace Roguish.ECS.Events;
internal class LevelChangeEvent(int newLevel)
{
    public int NewLevel { get; set; } = newLevel;
}
