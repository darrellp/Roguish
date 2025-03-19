using EcsRx.Groups.Observable;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;

// ReSharper disable once UnusedMember.Global
internal class NewDungeonSystem(DungeonSurface dungeon) : IReactToEventSystem<LevelChangeEvent>
{
    
    public IObservableGroup LevelItems = 
        Program.GetGroup(typeof(LevelItemComponent));

    public void Process(LevelChangeEvent eventData)
    {
        foreach (var item in LevelItems)
        {
            if (item.HasComponent(typeof(IsPlayerControlledComponent)) && item.HasComponent(typeof(DisplayComponent)))
            {
                var display = item.GetComponent(typeof(DisplayComponent)) as DisplayComponent;
                display.ScEntity.Position = dungeon.FindRandomEmptyPoint();
            }
        }
    }
}
