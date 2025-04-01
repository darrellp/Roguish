global using EcsEntity = EcsRx.Entities.IEntity;
global using ScEntity = SadConsole.Entities.Entity;
global using EcsComponent = EcsRx.Components.IComponent;
global using FOV = GoRogue.FOV.RecursiveShadowcastingBooleanBasedFOV;
global using static Roguish.Program;
using EcsRx.Groups.Observable;
using EcsRx.Infrastructure.Extensions;
using Ninject;
using Roguish.ECS;
using Roguish.Map_Generation;
using SadConsole.Configuration;
using SystemsRx.Infrastructure.Ninject.Extensions;
using Roguish.Screens;

namespace Roguish;
internal static class Program
{
    public static IKernel Kernel { get; private set; } = null!;
    public static readonly EcsRxApp EcsApp = new();
    public static FOV Fov = null!;

    // ReSharper disable once UnusedParameter.Global
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

    public static IObservableGroup GetGroup(params Type[] components)
    {
        return EcsApp.DependencyRegistry.BuildResolver().ResolveObservableGroup(components);
    }

    private static void DoBindings()
    {
        Kernel.Bind<StatusBar>().ToSelf().InSingletonScope();
        Kernel.Bind<DungeonSurface>().ToSelf().InSingletonScope();
        Kernel.Bind<TopContainer>().ToSelf().InSingletonScope();
        Kernel.Bind<MapGenerator>().ToSelf().InSingletonScope();
        Kernel.Bind<InfoBar>().ToSelf().InSingletonScope();
        Kernel.Bind<DescriptionSurface>().ToSelf().InSingletonScope();
    }

    private static Builder SetupGame()
    {
        Settings.AllowWindowResize = GameSettings.FAllowResize;
        Settings.ResizeMode = GameSettings.ResizeMode;
        Settings.ClearColor = GameSettings.ClearColor;

        return new Builder()
            .SetScreenSize(GameSettings.GameWidth, GameSettings.GameHeight)
            .ConfigureFonts("Fonts/Haberdash_curses_12x12.font")
            .OnStart(Start)
            .OnEnd(End);
    }
    private static void End(object? sender, GameHost e)
    {
        EcsApp.StopApplication();
    }

    private static void Start(object? sender, GameHost e)
    {
        EcsApp.StartApplication();

        var container = Kernel.Get<TopContainer>();
        Game.Instance.Screen = container;
        var ds = Kernel.Get<DungeonSurface>();
        ds.Position = new Point(GameSettings.IbWidth, 0);
        container.Children.Add(ds);
        var sb = Kernel.Get<StatusBar>();
        container.Children.Add(sb);
        var ib = Kernel.Get<InfoBar>();
        ib.Position = Point.Zero;
        container.Children.Add(ib);
        var dc = Kernel.Get<DescriptionSurface>();
        ib.Children.Add(dc);
        dc.Position = GameSettings.DescPosition;
        DrawDescBorder(ib);

        ds.FillSurface(ds);
        MVVM.Bindings.Bind();
    }

    private static void DrawDescBorder(InfoBar ib)
    {
        var rdescLeft = GameSettings.DescPosition.X - 1;
        var rdescTop = GameSettings.DescPosition.Y - 1;
        const int rdescWidth = GameSettings.DescWidth + 2;
        const int rdescHeight = GameSettings.DescHeight + 2;
        ib.DrawBox(new Rectangle(rdescLeft, rdescTop, rdescWidth, rdescHeight), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin,
            new ColoredGlyph(Color.Orange, Color.Black)));
    }
}