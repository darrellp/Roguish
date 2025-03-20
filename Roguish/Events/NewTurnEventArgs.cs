namespace Roguish.Events;
internal class NewTurnEventArgs(Point playerPosition) : EventArgs
{
    public Point PlayerPosition = playerPosition;
}
