global using EcsEntity = EcsRx.Entities.IEntity;
global using ScEntity = SadConsole.Entities.Entity;
global using EcsComponent = EcsRx.Components.IComponent;
global using static Roguish.Program;
using EcsRx.Groups.Observable;
using EcsRx.Infrastructure.Extensions;
using Ninject;
using Roguish.ECS;
using Roguish.Events;
using SadConsole.Configuration;
using SystemsRx.Infrastructure.Ninject.Extensions;
using Roguish.ECS.EcsEvents;
using SystemsRx.Events;

namespace Roguish;
internal class Program
{
    public static IKernel Kernel { get; set; } = null!;
    public static EcsRxApp EcsApp = new();
    public static event NewTurnEventHandler? NewTurnEvent;
    public delegate void NewTurnEventHandler(object sender, NewTurnEventArgs e);

    public static void Main(string[] args)
    {
        Kernel = EcsApp.DependencyRegistry.GetKernel();
        DoBindings();

        Settings.WindowTitle = "My SadConsole Game";

        var gameConfig = SetupGame();

        Game.Create(gameConfig);
        Game.Instance.Run();
        Game.Instance.Dispose();
    }

    public static void OnNewTurn(object sender)
    {
        var playerPosition = EcsApp.PlayerPos;
        EcsApp.Get<IEventSystem>().Publish(new NewTurnEvent(playerPosition));
        var eventArgs = new NewTurnEventArgs(playerPosition);
        NewTurnEvent?.Invoke(sender, eventArgs);
    }

    public static IObservableGroup GetGroup(params Type[] Components)
    {
        return EcsApp.DependencyRegistry.BuildResolver().ResolveObservableGroup(Components);
    }

    private static void DoBindings()
    {
        Kernel.Bind<StatusBar>().ToSelf().InSingletonScope();
        Kernel.Bind<DungeonSurface>().ToSelf().InSingletonScope();
        Kernel.Bind<TopContainer>().ToSelf().InSingletonScope();
    }

    public static Builder SetupGame()
    {
        Settings.AllowWindowResize = GameSettings.FAllowResize;
        Settings.ResizeMode = GameSettings.ResizeMode;
        Settings.ClearColor = GameSettings.ClearColor;

        return new Builder()
            .SetScreenSize(GameSettings.GameWidth, GameSettings.GameHeight)
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