global using EcsEntity = EcsRx.Entities.Entity;
global using ScEntity = SadConsole.Entities.Entity;
global using EcsComponent = EcsRx.Components.IComponent;

using Ninject;
using Roguish.ECS;
using SystemsRx.Infrastructure.Ninject.Extensions;

namespace Roguish;
internal class Program
{
    public static IKernel Kernel { get; set; }
    public static EcsRxApp EcsApp = new();

    public static void Main(string[] args)
    {
        Kernel = EcsApp.DependencyRegistry.GetKernel();
        RebindAsSingletons();

        Settings.WindowTitle = "My SadConsole Game";

        var settings = Kernel.Get<GameSettings>();
        var gameConfig = settings.SetupGame();

        Game.Create(gameConfig);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    // These are not bound as singletons by EcsRx so we have to unbind them and rebind as singletons
    private static void RebindAsSingletons()
    {
        RebindAsSingleton<StatusBar>();
        RebindAsSingleton<DungeonSurface>();
        RebindAsSingleton<GameSettings>();
        RebindAsSingleton<TopContainer>();

    }

    private static void RebindAsSingleton<T>() where T : class
    {
        Program.Kernel.Unbind<T>();
        Program.Kernel.Bind<T>().ToSelf().InSingletonScope();
    }

}