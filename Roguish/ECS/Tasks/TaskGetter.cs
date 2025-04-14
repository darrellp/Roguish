using Roguish.Map_Generation;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Screens;
// ReSharper disable IdentifierTypo
namespace Roguish.ECS.Tasks;

public enum TaskType
{
    Unassigned,
    PlayerMove,
    AgentMove,
    AgentPursue,
    PickUp,
    Equip,
    Regenerate,
    TakeStairs,
    Drop,

    // Not implemented yet
    Use,
    Wait,
    Attack,
    CastSpell,
    Rest,
    WaitForTurn
}

internal static partial class TaskGetter
{
    #region Action Table
    // Parallels TaskType enum above...
    internal static readonly List<Action<EcsEntity, RogueTask>> ActionTable =[
        null,
        MovePlayer,
        DefaultAgentMove,
        AgentPursue,
        UserPickup,
        UserEquip,
        Regenerate,
        TakeStairs,
        UserDrop,
    ];
    #endregion

    #region Fields
    internal static ulong Ticks { get; set; } = 0;
    private static readonly DungeonSurface Dungeon;
    private static readonly MapGenerator Mapgen;
    private static readonly LogScreen Log;
    private static readonly InventorySurface Inv;
    #endregion

    #region Task Times
    internal const int StdMovementTime = 100;
    internal const int PickUpTime = 10;
    internal const int EquipTime = 10;
    #endregion

    #region (static) Constructor
    static TaskGetter()
    {
        Dungeon = Kernel.Get<DungeonSurface>();
        Mapgen = Kernel.Get<MapGenerator>();
        Log = Kernel.Get<LogScreen>();
        Inv = Kernel.Get<InventorySurface>();
    }
    #endregion
}
