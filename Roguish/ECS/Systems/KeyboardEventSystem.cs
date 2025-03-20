using Roguish.ECS.Components;
using Roguish.ECS.EcsEvents;
using SadConsole.Input;
using SystemsRx.Events;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;
internal class KeyboardEventSystem(DungeonSurface dungeon, IEventSystem eventSystem) : IReactToEventSystem<KeyboardEvent>
{
    public void Process(KeyboardEvent keyData)
    {
        if (keyData.Keys.Count != 1)
        {
            // We currently only handle single key presses
            return;
        }

        bool fNewTurn = true;
        var key = keyData.Keys[0].Key;
        switch (key)
        {
            case Keys.Up:
                MovePlayer(new Point(0, -1));
                break;

            case Keys.Down:
                MovePlayer(new Point(0, 1));
                break;

            case Keys.Left:
                MovePlayer(new Point(-1, 0));
                break;

            case Keys.Right:
                MovePlayer(new Point(1, 0));
                break;

            case Keys.PageUp:
                MovePlayer(new Point(1, -1));
                break;

            case Keys.PageDown:
                MovePlayer(new Point(1, 1));
                break;

            case Keys.Home:
                MovePlayer(new Point(-1, -1));
                break;

            case Keys.End:
                MovePlayer(new Point(-1, 1));
                break;

            case Keys.D5:
                break;

            default:
                fNewTurn = false;
                break;
        }

        if (fNewTurn)
        {
            Program.OnNewTurn(this);
        }
    }

    private void MovePlayer(Point ptMove)
    {
        var player = EcsApp.PlayerGroup.First();
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        var position = positionCmp.Position.Value;
        var newPosition = position + ptMove;
        if (dungeon.IsWalkable(newPosition))
        {
            positionCmp.Position.SetValueAndForceNotify(newPosition);
        }
    }
}
