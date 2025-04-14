using System.Collections.Concurrent;
using System.Diagnostics;
using EcsRx.Extensions;
using Ninject;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.ECS.Tasks;
using Roguish.Map_Generation;
using Roguish.Screens;
using Roguish.Serialization;
using SadConsole.Input;
using SystemsRx.Systems.Conventional;
// ReSharper disable IdentifierTypo

namespace Roguish.ECS.Systems;

// ReSharper disable once ClassNeverInstantiated.Global
internal class KeyboardEventSystem() : IReactToEventSystem<KeyboardEvent>
{
    #region Private Variables
    private static MapGenerator Mapgen = null!;
    private static LogScreen Log = null!;
    private static ConcurrentQueue<Keys> KeysQueue { get; } = new();
    #endregion

    #region Static Constructor
    static KeyboardEventSystem()
    {
        Mapgen = Kernel.Get<MapGenerator>();
        Log = Kernel.Get<LogScreen>();
    }
    #endregion

    #region Processing
    public void Process(KeyboardEvent keyData)
    {
        if (keyData.RetrieveFromQueue)
        {
            System.Threading.Tasks.Task.Factory.StartNew(ReadFromQueue);
            return;
        }

        StopQueue();

        if (keyData.Keys is not { Count: 1 })
        {
            // We currently only handle single key presses
            return;
        }
        var key = keyData.Keys[0].Key;
        ProcessKey(key);
    }
    private void ProcessKey(Keys key)
    {
        Point? ptMove = null;
        RogueTask? task = null;
        var player = EcsRxApp.Player;
        Debug.Assert(player != null);
        var kb = SadConsole.Game.Instance.Keyboard;

        switch (key)
        {
            case Keys.S:
                if (kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl))
                {
                    StopQueue();
                    Serialize.Test();
                }
                break;

            case Keys.OemPeriod:
                if (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift))
                {
                    var (entity, fMore) = Mapgen.GetEntityAt(EcsApp.PlayerPos, true);
                    // TODO: Handle fMore
                    if (entity == null || !entity.HasComponent<StairsComponent>())
                    {
                        
                        return;
                    }
                    task = TaskGetter.CreateTakeStairsTask(TaskGetter.Ticks);
                }
                break;

            case Keys.D:
                task = TaskGetter.CreateDropTask();
                break;

            case Keys.G:
                task = TaskGetter.CreatePickupTask();
                break;

            case Keys.E:
                task = TaskGetter.CreateEquipTask();
                break;

            case Keys.D5:
                ptMove = new Point(0, 0);
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

        if (ptMove != null)
        {
            var newPosition = NextPos(player, ptMove.Value);
            if (!MapGenerator.IsWalkable(newPosition))
            {
                return;
            }

            task = TaskGetter.CreatePlayerMoveTask(newPosition);

        }

        if (task != null)
        {
            var taskCmp = player.GetComponent<TaskComponent>();
            taskCmp.Tasks[0] = task;
            EcsApp.EventSystem.Publish(new NewTurnEvent());
        }
    }

    private Point NextPos(EcsEntity player, Point ptMoveDelta)
    {
        var positionCmp = (PositionComponent)player.GetComponent(typeof(PositionComponent));
        var position = positionCmp.Position.Value;
        return position + ptMoveDelta;
    }
    #endregion

    #region Queueing
    internal static bool HasQueue()
    {
        return KeysQueue.Count > 0;
    }

    internal static void StopQueue()
    {
        KeysQueue.Clear();
    }

    internal static void EnqueueKey(Keys key)
    {
        KeysQueue.Enqueue(key);
    }

    private void ReadFromQueue()
    {
        while (KeysQueue.TryDequeue(out var key))
        {
            ProcessKey(key);
            Thread.Sleep(50);
        }
    }
    #endregion

}