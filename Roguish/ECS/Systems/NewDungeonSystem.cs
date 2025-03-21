using System.Diagnostics;
using Roguish.ECS.Components;
using Roguish.ECS.Events;
using Roguish.Map_Generation;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;

// ReSharper disable once UnusedMember.Global
internal class NewDungeonSystem(MapGenerator mapgen, DungeonSurface dungeon) : IReactToEventSystem<NewDungeonEvent>
{
    public void Process(NewDungeonEvent eventData)
    {
        mapgen.Generate();
        foreach (var item in EcsApp.LevelItems)
        {
            if (item.HasComponent(typeof(IsPlayerControlledComponent)) && item.HasComponent(typeof(DisplayComponent)))
            {
                if (item.HasComponent(typeof(PositionComponent)))
                {
                    var posCmp = item.GetComponent(typeof(PositionComponent)) as PositionComponent;
                    Debug.Assert(dungeon != null, nameof(dungeon) + " != null");
                    posCmp!.Position.SetValueAndForceNotify(dungeon.FindRandomEmptyPoint());
                }
            }
        }
    }
}
