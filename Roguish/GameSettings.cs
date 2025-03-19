using Ninject;
using Roguish.ECS;
using SadConsole.Configuration;
using SystemsRx.Infrastructure.Ninject.Extensions;

namespace Roguish;

public class GameSettings
{
    public static int GameWidth { get; }= 90;
    public static int GameHeight { get; }= 41;

    public static int DungeonWidth { get; } = 300;
    public static int DungeonHeight { get; } = 200;

    public static int DungeonViewWidth { get; } = GameWidth;
    public static int DungeonViewHeight { get; } = GameHeight - 1;

    public static int BorderWidthX = 7;
    public static int BorderWidthY = 5;

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
                .OnStart(Start)
                .OnEnd(End);
    }

    private static void End(object? sender, GameHost e)
    {
        Program.EcsApp.StopApplication();
    }

    private static void Start(object? sender, GameHost e)
    {
        Program.EcsApp.StartApplication();

        var container = Program.Kernel.Get<TopContainer>();
        Game.Instance.Screen = container; 
        var ds = Program.Kernel.Get<DungeonSurface>();
        container.Children.Add(ds);
        var sb = Program.Kernel.Get<StatusBar>();
        container.Children.Add(sb);

        ds.FillSurface(ds);
        MVVM.Bindings.Bind();
    }
}