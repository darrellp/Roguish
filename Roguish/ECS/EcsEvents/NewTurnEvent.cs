namespace Roguish.ECS.EcsEvents;
internal class NewTurnEvent(Point playerPosition)
{
    public Point PlayerPosition { get; set; } = playerPosition;
}
