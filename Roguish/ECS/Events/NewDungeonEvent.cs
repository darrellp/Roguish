namespace Roguish.ECS.Events;
internal class NewDungeonEvent(int newLevel)
{
    public int NewLevel { get; set; } = newLevel;
}
