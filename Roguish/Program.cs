using Ninject;

namespace Roguish;
internal class Program
{
    public static StandardKernel Kernel { get; } = new();

    public static void Main(string[] args)
    {
        SetupIoc();
        Settings.WindowTitle = "My SadConsole Game";

        var settings = Kernel.Get<GameSettings>();
        var gameConfig = settings.SetupGame();

        Game.Create(gameConfig);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    private static void SetupIoc()
    {
        Kernel.Bind<GameSettings>().ToSelf().InSingletonScope();
        //Kernel.Bind<RootScreen>().ToSelf().InSingletonScope();
        Kernel.Bind<DungeonSurface>().ToSelf().InSingletonScope();
        Kernel.Bind<StatusBar>().ToSelf().InSingletonScope();
        Kernel.Bind<TopContainer>().ToSelf().InSingletonScope();
    }
}