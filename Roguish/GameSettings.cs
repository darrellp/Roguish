using Ninject;
using SadConsole.Configuration;

namespace Roguish;

public class GameSettings
{
    public static int GameWidth { get; }= 90;
    public static int GameHeight { get; }= 41;

    public int DungeonWidth { get; } = GameWidth;
    public int DungeonHeight { get; } = GameHeight - 1;

    public int SbWidth = GameWidth;
    public int SbHeight = 1;
    public Point SbPosition = new(0, GameHeight - 1);

    public bool FAllowResize { get; } = true;
    public bool FResizeHook = true;
    public Settings.WindowResizeOptions ResizeMode { get; } = Settings.WindowResizeOptions.None;
    public Color ClearColor = Color.Black;
    public Color ForeColor = Color.Orange;

    public Builder SetupGame()
    {
        Settings.AllowWindowResize = FAllowResize;
        Settings.ResizeMode = ResizeMode;
        Settings.ClearColor = ClearColor;

        return new Builder()
                .SetScreenSize(GameWidth, GameHeight)
                .OnStart(Start);
    }

    private static void Start(object? sender, GameHost e)
    {
        var container = Program.Kernel.Get<TopContainer>();
        Game.Instance.Screen = Program.Kernel.Get<TopContainer>(); 
        var ds = Program.Kernel.Get<DungeonSurface>();
        container.Children.Add(ds);
        var sb = Program.Kernel.Get<StatusBar>();
        container.Children.Add(sb);

        ds.FillSurface();
    }
}