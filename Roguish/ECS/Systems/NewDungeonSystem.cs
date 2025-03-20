using System.Diagnostics;
using Roguish.ECS.Components;
using Roguish.ECS.EcsEvents;
using SystemsRx.Systems.Conventional;

namespace Roguish.ECS.Systems;

// ReSharper disable once UnusedMember.Global
internal class NewDungeonSystem(DungeonSurface dungeon) : IReactToEventSystem<LevelChangeEvent>
{
    public void Process(LevelChangeEvent eventData)
    {
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
        Program.OnNewTurn(this);
    }
}
