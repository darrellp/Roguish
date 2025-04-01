using System.Collections.Concurrent;
using System.Diagnostics;
using EcsRx.Extensions;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using Roguish.Screens;
using SadConsole.Input;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;

// ReSharper disable once ClassNeverInstantiated.Global
internal class KeyboardEventSystem(DungeonSurface dungeon) : IReactToEventSystem<KeyboardEvent>
{
    public static ConcurrentQueue<Keys> KeysQueue { get; } = new();

    public void Process(KeyboardEvent keyData)
    {
        if (keyData.RetrieveFromQueue)
        {
            Task.Factory.StartNew(ReadFromQueue);
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
        var ptMove = Point.Zero;

        switch (key)
        {
            case Keys.D5:
                break;

            case Keys.Up:
                ptMove = new Point(0, -1);
                break;

            case Keys.Down:
                ptMove = new Point(0, 1);
                break;

            case Keys.Left:
                ptMove = new Point(-1, 0);
                break;

            case Keys.Right:
                ptMove = new Point(1, 0);
                break;

            case Keys.PageUp:
                ptMove = new Point(1, -1);
                break;

            case Keys.PageDown:
                ptMove = new Point(1, 1);
                break;

            case Keys.Home:
                ptMove = new Point(-1, -1);
                break;

            case Keys.End:
                ptMove = new Point(-1, 1);
                break;
        }

        var player = EcsApp.PlayerGroup.First();
        Debug.Assert(player != null);
        var newTicks = NewTurnEventSystem.Ticks + 100ul;
        var newPosition = NextPos(player, ptMove);
        if (!MapGenerator.IsWalkable(newPosition))
        {
            return;
        }
        var action = MovePlayerClosure(newPosition);
        var newTask = new TaskComponent(newTicks, action);
        player.AddComponent(newTask);
        EcsApp.EventSystem.Publish(new NewTurnEvent());
    }

    private Action<EcsEntity> MovePlayerClosure(Point newPosition)
    {
        return _ => { MovePlayer(newPosition); };
    }

    private void MovePlayer(Point newPosition)
    {
        var player = EcsApp.PlayerGroup.First();
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        Debug.Assert(MapGenerator.IsWalkable(newPosition));
        positionCmp.Position.SetValueAndForceNotify(newPosition);
        dungeon.KeepPlayerInView();
    }

    private Point NextPos(EcsEntity player, Point ptMoveDelta)
    {
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        var position = positionCmp.Position.Value;
        return position + ptMoveDelta;
    }
}