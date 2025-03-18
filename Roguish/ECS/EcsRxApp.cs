using EcsRx.Groups.Observable;
using EcsRx.Infrastructure;
using EcsRx.Plugins.Views;
using Ninject;
using Roguish.ECS.Components;
using SystemsRx.Events;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Ninject;
using SystemsRx.Infrastructure.Ninject.Extensions;

namespace Roguish.ECS;

internal class EcsRxApp : EcsRxApplication
{
    protected override void ApplicationStarted()
    {
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

    protected override void LoadPlugins()
    {
        RegisterPlugin(new ViewsPlugin());
    }

    public override IDependencyRegistry DependencyRegistry { get; } = new NinjectDependencyRegistry();

    public T Get<T>()
    {
        return DependencyRegistry.GetKernel().Get<T>();
    }
}