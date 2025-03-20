using Ninject;
using SadConsole.Configuration;

namespace Roguish;

public static class GameSettings
{
    #region Positioning
    // Starting size for the entire game
    public const int GameWidth = 90;
    public const int GameHeight = 41;

    // Size of the dungeon
    public const int DungeonWidth = 300;
    public const int DungeonHeight = 80;

    // Starting size of the window into the dungeon
    public static int DungeonViewWidth { get; } = GameWidth;
    public static int DungeonViewHeight { get; } = GameHeight - 1;

    // How close we can walk to the edges before scrolling starts
    public const int BorderWidthX = 7;
    public const int BorderWidthY = 5;

    // Status bar size
    public static int SbWidth { get; } = GameWidth;
    public const int SbHeight = 1;
    public static Point SbPosition { get; } = new(0, GameHeight - 1);
    #endregion

    #region Misc Settings
    public static bool FAllowResize => true;
    public static Settings.WindowResizeOptions ResizeMode => Settings.WindowResizeOptions.None;
    public static Color ClearColor = Color.Black;
    public static Color ForeColor = Color.Orange;
    #endregion
}