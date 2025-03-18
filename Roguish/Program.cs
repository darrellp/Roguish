﻿global using EcsEntity = EcsRx.Entities.Entity;
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
        DoBindings();

        Settings.WindowTitle = "My SadConsole Game";

        var settings = Kernel.Get<GameSettings>();
        var gameConfig = settings.SetupGame();

        Game.Create(gameConfig);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    private static void DoBindings()
    {
        Kernel.Bind<StatusBar>().ToSelf().InSingletonScope();
        Kernel.Bind<DungeonSurface>().ToSelf().InSingletonScope();
        Kernel.Bind<GameSettings>().ToSelf().InSingletonScope();
        Kernel.Bind<TopContainer>().ToSelf().InSingletonScope();
    }
}