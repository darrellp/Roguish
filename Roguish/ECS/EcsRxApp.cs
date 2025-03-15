using EcsRx.Infrastructure;
using EcsRx.Plugins.Views;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Ninject;

namespace Roguish.ECS;

internal class EcsRxApp : EcsRxApplication
{
    protected override void ApplicationStarted()
    {
        var collection = EntityDatabase.GetCollection();
        var entity = collection.CreateEntity();
    }

    protected override void LoadPlugins()
    {
        RegisterPlugin(new ViewsPlugin());
    }

    public override IDependencyRegistry DependencyRegistry { get; } = new NinjectDependencyRegistry();
}