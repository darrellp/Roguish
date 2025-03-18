using Roguish.ECS.Events;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;

internal class NewDungeonSystem : IReactToEventSystem<LevelChangeEvent>
{
    public void Process(LevelChangeEvent eventData)
    {
    }
}
