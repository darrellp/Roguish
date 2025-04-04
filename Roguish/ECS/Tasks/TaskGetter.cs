using Roguish.ECS.Components;
using Roguish.Map_Generation;
using System.Diagnostics;
using EcsRx.Extensions;
using Ninject;
using Roguish.Screens;
using GoRogue.Random;
// ReSharper disable IdentifierTypo

namespace Roguish.ECS.Tasks;
internal static partial class TaskGetter
{
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
