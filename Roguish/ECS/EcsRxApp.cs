using System.Diagnostics;
using EcsRx.Groups.Observable;
using EcsRx.Infrastructure;
using EcsRx.Plugins.Views;
using Ninject;
using Roguish.ECS.Components;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Ninject;
using SystemsRx.Infrastructure.Ninject.Extensions;
using EcsRx.Extensions;
// ReSharper disable IdentifierTypo

namespace Roguish.ECS;

internal class EcsRxApp : EcsRxApplication
{
    public IObservableGroup PlayerGroup = null!;
    public IObservableGroup LevelItems = null!;
    public IObservableGroup EnemiesGroup = null!;

    protected override void ApplicationStarted()
    {
        PlayerGroup = GetGroup(typeof(IsPlayerControlledComponent));
        LevelItems = GetGroup(typeof(LevelItemComponent));
        EnemiesGroup = GetGroup(typeof(EnemyComponent));

        var collection = EntityDatabase.GetCollection();
        var dungeon = Program.Kernel.Get<DungeonSurface>();
        var entity = collection.CreateEntity(MonsterInfo.GetPlayerBlueprint(20, dungeon));
        entity.AddComponent(new IsPlayerControlledComponent());
    }

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

    protected override void LoadPlugins()
    {
        RegisterPlugin(new ViewsPlugin());
    }

    public override IDependencyRegistry DependencyRegistry { get; } = new NinjectDependencyRegistry();

    // ReSharper disable once UnusedMember.Global
    public T Get<T>()
    {
        return DependencyRegistry.GetKernel().Get<T>();
    }
}