using System.Diagnostics;
using EcsRx.Extensions;
using EcsRx.Groups.Observable;
using EcsRx.Infrastructure;
using EcsRx.Plugins.Views;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Info;
using Roguish.Screens;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Ninject;
using SystemsRx.Infrastructure.Ninject.Extensions;

// ReSharper disable IdentifierTypo

namespace Roguish.ECS;

internal class EcsRxApp : EcsRxApplication
{
    public static IObservableGroup LevelItems = null!;
    public static IObservableGroup PlayerGroup = null!;
    public static IObservableGroup TaskedGroup = null!;
    public static IObservableGroup DisplayGroup = null!;
    internal static EcsEntity Player = null!;

    public Point PlayerPos
    {
        get
        {
            var poscmp = Player.GetComponent<PositionComponent>();
            Debug.Assert(poscmp != null);
            return poscmp.Position.Value;
        }
    }

    public override IDependencyRegistry DependencyRegistry { get; } = new NinjectDependencyRegistry();

    protected override void ApplicationStarted()
    {
        PlayerGroup = GetGroup(typeof(IsPlayerControlledComponent));
        LevelItems = GetGroup(typeof(LevelItemComponent));
        GetGroup(typeof(AgentTypeComponent));
        TaskedGroup = GetGroup(typeof(TaskComponent));
        DisplayGroup = GetGroup(typeof(DisplayComponent), typeof(PositionComponent));
        var collection = EntityDatabase.GetCollection();
        var dungeon = Kernel.Get<DungeonSurface>();
        Player = collection.CreateEntity(AgentInfo.GetPlayerBlueprint(20, dungeon));
    }

    protected override void LoadPlugins()
    {
        RegisterPlugin(new ViewsPlugin());
    }

    // ReSharper disable once UnusedMember.Global
    public T Get<T>()
    {
        return DependencyRegistry.GetKernel().Get<T>();
    }
}