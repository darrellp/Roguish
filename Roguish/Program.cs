global using EcsEntity = EcsRx.Entities.Entity;
global using EcsComponent = EcsRx.Components.IComponent;

using Ninject;
using Roguish.ECS;

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
        Kernel.Bind<DungeonSurface>().ToSelf().InSingletonScope();
        Kernel.Bind<StatusBar>().ToSelf().InSingletonScope();
        Kernel.Bind<TopContainer>().ToSelf().InSingletonScope();
        Kernel.Bind<EcsRxApp>().ToSelf().InSingletonScope();
    }
}