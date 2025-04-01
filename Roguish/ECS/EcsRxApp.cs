using System.Diagnostics;
using EcsRx.Extensions;
using EcsRx.Groups.Observable;
using EcsRx.Infrastructure;
using EcsRx.Plugins.Views;
using Ninject;
using Roguish.ECS.Components;
using Roguish.Screens;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Ninject;
using SystemsRx.Infrastructure.Ninject.Extensions;

// ReSharper disable IdentifierTypo

namespace Roguish.ECS;

internal class EcsRxApp : EcsRxApplication
{
    public IObservableGroup LevelItems = null!;
    public IObservableGroup PlayerGroup = null!;
    public IObservableGroup TaskedGroup = null!;

    public Point PlayerPos
    {
        get
        {
            var player = PlayerGroup[0];
            var poscmp = player.GetComponent(typeof(PositionComponent)) as PositionComponent;
            Debug.Assert(poscmp != null);
            return poscmp.Position.Value;
        }
    }

    public override IDependencyRegistry DependencyRegistry { get; } = new NinjectDependencyRegistry();

    protected override void ApplicationStarted()
    {
        PlayerGroup = GetGroup(typeof(IsPlayerControlledComponent));
        LevelItems = GetGroup(typeof(LevelItemComponent));
        GetGroup(typeof(AgentComponent));
        TaskedGroup = GetGroup(typeof(TaskComponent));

        var collection = EntityDatabase.GetCollection();
        var dungeon = Kernel.Get<DungeonSurface>();
        var entity = collection.CreateEntity(AgentInfo.GetPlayerBlueprint(20, dungeon));
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