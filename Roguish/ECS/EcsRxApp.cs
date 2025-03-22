using System.Diagnostics;
using EcsRx.Groups.Observable;
using EcsRx.Infrastructure;
using EcsRx.Plugins.Views;
using Ninject;
using Roguish.ECS.Components;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Ninject;
using SystemsRx.Infrastructure.Ninject.Extensions;
// ReSharper disable IdentifierTypo

namespace Roguish.ECS;

internal class EcsRxApp : EcsRxApplication
{
    public IObservableGroup PlayerGroup = null!;
    public IObservableGroup LevelItems = null!;

    protected override void ApplicationStarted()
    {
        PlayerGroup = Program.GetGroup(typeof(IsPlayerControlledComponent));
        LevelItems = Program.GetGroup(typeof(LevelItemComponent));

        var collection = EntityDatabase.GetCollection();
        var entity = collection.CreateEntity();
        var dungeon = Program.Kernel.Get<DungeonSurface>();
        var playerPos = new Point(0, 0);
        var scePlayer = dungeon.CreateScEntity(new ColoredGlyph(Color.White, Color.Transparent, 0x40), playerPos, '@', int.MaxValue);
        IReadOnlyList<EcsComponent> components = new List<EcsComponent>
        {
            new IsPlayerControlledComponent(),
            new DescriptionComponent("Player", "It's you silly!"),
            new PositionComponent( playerPos),
            new DisplayComponent(scePlayer),
            new LevelItemComponent(),
        };

        entity.AddComponents(components);
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