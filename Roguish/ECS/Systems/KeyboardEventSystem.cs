using System.Collections.Concurrent;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SadConsole.Input;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;
internal class KeyboardEventSystem(DungeonSurface dungeon) : IReactToEventSystem<KeyboardEvent>
{
    public static ConcurrentQueue<Keys> KeysQueue { get; set; } = new();
    
    public void Process(KeyboardEvent keyData)
    {
        if (keyData.RetrieveFromQueue)
        {
            var task = Task.Factory.StartNew(ReadFromQueue);
            return;
        }
        
        KeysQueue.Clear();

        if (keyData.Keys is not { Count: 1 })
        {
            // We currently only handle single key presses
            return;
        }
        var key = keyData.Keys[0].Key;
        ProcessKey(key);
    }

    private void ReadFromQueue()
    {
        while (KeysQueue.TryDequeue(out var key))
        {
            ProcessKey(key);
            Thread.Sleep(50);
        }
    }

    private void ProcessKey(Keys key)
    {
        if (key == Keys.D5)
        {
            DungeonSurface.SignalNewFov(false);
        }
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
        }
        dungeon.KeepPlayerInView();
    }

    private void MovePlayer(Point ptMove)
    {
        var player = EcsApp.PlayerGroup.First();
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        var position = positionCmp.Position.Value;
        var newPosition = position + ptMove;
        if (MapGenerator.IsWalkable(newPosition))
        {
            positionCmp.Position.SetValueAndForceNotify(newPosition);
        }
    }
}
