using EcsR3.Plugins.GroupBinding.Attributes;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Systems;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using SystemsRx.Systems.Conventional;
using EcsRx.Collections;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Systems;


namespace Roguish.ECS.Systems;

internal class NewDungeonSystem : IReactToEventSystem<LevelChangeEvent>
{
    [FromComponents(typeof(LevelItemComponent))]
    public IObservableGroup LevelItems;

    private readonly DungeonSurface _dungeon;

    public NewDungeonSystem(DungeonSurface dungeon)
    {
        _dungeon = dungeon;
    }

    public void Process(LevelChangeEvent eventData)
    {
        //foreach (var item in LevelItems)
        //{
        //    if (item.HasComponent(typeof(DisplayComponent)))
        //    {
        //        var display = item.GetComponent(typeof(DisplayComponent)) as DisplayComponent;
        //        display.ScEntity.Position = _dungeon.FindRandomEmptyPoint();
        //    }
        //}
    }
}
