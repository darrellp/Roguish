﻿using GoRogue.Random;

namespace Roguish;

internal static class GameSettings
{
    #region Positioning
    // Starting size for the entire game
    public const int GameWidth = 130;
    public const int GameHeight = 91;

    // Size of the dungeon
    public const int DungeonWidth = 90;
    public const int DungeonHeight = GameHeight - 1;

    // Starting size of the window into the dungeon
    public const int DungeonViewWidth = DungeonWidth;
    public const int DungeonViewHeight = DungeonHeight;

    public const int IbWidth = GameWidth - DungeonViewWidth;
    public const int IbHeight = GameHeight - 1;
    public static Point IbPosition { get; } = Point.Zero;

    // How close we can walk to the edges before scrolling starts
    public const int BorderWidthX = 7;
    public const int BorderWidthY = 7;

    // Status bar size
    public const int SbWidth = GameWidth;
    public const int SbHeight = 1;
    public static Point SbPosition { get; } = new(0, GameHeight - 1);

    // Description console info
    public const int DescWidth = IbWidth - 2;
    public const int DescHeight = 3;
    public static Point DescPosition { get; } = new(1, GameHeight - DescHeight - SbHeight - 1);
    #endregion

    #region Misc Settings
    public const Settings.WindowResizeOptions ResizeMode = Settings.WindowResizeOptions.None;
    public const int FovRadius = 10;

    public static int AgentsPerLevel => GlobalRandom.DefaultRNG.NextInt(AgentsPerLevelMin, AgentsPerLevelMax + 1);
    public const int AgentsPerLevelMin = 15;
    public const int AgentsPerLevelMax = 20;
    public static int WeaponsPerLevel => GlobalRandom.DefaultRNG.NextInt(WeaponsPerLevelMin, WeaponsPerLevelMax + 1);
    public const int WeaponsPerLevelMin = 5;
    public const int WeaponsPerLevelMax = 10;
    public static bool FAllowResize => false;
    public static Color ClearColor = Color.Black;
    public static Color FloorColor = Color.Orange;
    public static Color WallColor = Color.Blue;
    #endregion
}