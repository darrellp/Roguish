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
// ReSharper disable StringLiteralTypo

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
        Kernel.Bind<LogScreen>().ToSelf().InSingletonScope();
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
        DrawInfoBarBorder(ib);

        var log = Kernel.Get<LogScreen>();
        ib.Children.Add(log);
        log.Position = GameSettings.LogPosition;
        log.PrintProcessedString("""
                         ***************************
                         [c:r f:Orange]Welcome to Roguish!
                         Hope you enjoy the game![c:undo]
                         ***************************
                         """);

        ds.FillSurface(ds);
        MVVM.Bindings.Bind();
    }

    private static void DrawInfoBarBorder(InfoBar ib)
    {
        ib.DrawLine(new Point(0, 0), new Point(0, GameSettings.IbHeight - 1), DungeonSurface.pathVert.Glyph);
        ib.DrawLine(new Point(GameSettings.IbWidth - 1, 0), new Point(GameSettings.IbWidth - 1, GameSettings.IbHeight - 1), DungeonSurface.pathVert.Glyph);
        foreach (var yBar in GameSettings.IbCrossBars)
        {
            ib.DrawLine(new Point(0, yBar), new Point(GameSettings.IbWidth, yBar), DungeonSurface.pathHoriz.Glyph);
            ib.SetGlyph(0, yBar, DungeonSurface.pathTLeft.Glyph);
            ib.SetGlyph(GameSettings.IbWidth - 1, yBar, DungeonSurface.pathTRight.Glyph);
        }
        ib.SetGlyph(0, 0, DungeonSurface.pathUL.Glyph);
        ib.SetGlyph(GameSettings.IbWidth - 1, 0, DungeonSurface.pathUR.Glyph);
        ib.SetGlyph(0, GameSettings.IbHeight - 1, DungeonSurface.pathLL.Glyph);
        ib.SetGlyph(GameSettings.IbWidth - 1, GameSettings.IbHeight - 1, DungeonSurface.pathLR.Glyph);
    }
}