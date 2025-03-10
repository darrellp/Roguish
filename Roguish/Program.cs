using Roguish;
using SadConsole.Configuration;
using Ninject;
using SadConsole.UI.Controls;

internal class Program
{
    public static StandardKernel Kernel { get; } = new Ninject.StandardKernel();

    public static void Main(string[] args)
    {
        SetupIoc();
        Settings.WindowTitle = "My SadConsole Game";

        var settings = Kernel.Get<GameSettings>();
        Builder gameConfig = settings.SetupGame();

        Game.Create(gameConfig);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    private static void SetupIoc()
    {
        Kernel.Bind<GameSettings>().ToSelf().InSingletonScope();
        Kernel.Bind<RootScreen>().ToSelf().InSingletonScope();
        Kernel.Bind<DungeonSurface>().ToSelf().InSingletonScope();
        Kernel.Bind<StatusBar>().ToSelf().InSingletonScope();
    }
}

