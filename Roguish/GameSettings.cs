using SadConsole.Configuration;

namespace Roguish;

internal static class GameSettings
{
    public static int GameWidth { get; }= 90;
    public static int GameHeight { get; }= 41;

    public static int DungeonWidth { get; } = GameWidth;
    public static int DungeonHeight { get; } = GameHeight - 1;

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
                .OnStart(Start);
    }

    private static void Start(object? sender, GameHost e)
    {
        var container = new ScreenObject();
        container.UseMouse = false;
        Game.Instance.Screen = container;
        var rs = new RootScreen();
        container.Children.Add(rs);
        var sb = new StatusBar(GameWidth, 1);
        sb.Position = new Point(0, GameHeight - 1);
        container.Children.Add(sb);
    }
}