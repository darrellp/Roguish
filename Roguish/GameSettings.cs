using SadConsole.Configuration;

namespace Roguish;

internal static class GameSettings
{

    public static int GameWidth { get; }= 90;
    public static int GameHeight { get; }= 30;
    public static bool FAllowResize { get; } = true;
    public static bool FResizeHook = true;
    public static Settings.WindowResizeOptions ResizeMode { get; } = Settings.WindowResizeOptions.None;
    public static Color ClearColor = Color.BurlyWood;
    public static Color ForeColor = Color.Black;

    public static Builder SetupGame()
    {
        Settings.AllowWindowResize = GameSettings.FAllowResize;
        Settings.ResizeMode = ResizeMode;
        Settings.ClearColor = ClearColor;

        return new Builder()
                .SetScreenSize(GameSettings.GameWidth, GameSettings.GameHeight)
                .SetStartingScreen<RootScreen>()
                .IsStartingScreenFocused(true)
                .ConfigureFonts(true);
    }
}