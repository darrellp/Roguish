using Roguish.ECS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguish.ECS.Components;
using SystemsRx.Systems.Conventional;
using EcsRx.Extensions;
using GoRogue.Random;
using ShaiRandom.Generators;
using Roguish.Map_Generation;


namespace Roguish.ECS.Systems;
internal class NewTurnEventSystem : IReactToEventSystem<NewTurnEvent>
{
    private DungeonSurface _dungeon = null!;
    private readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;


    public NewTurnEventSystem(DungeonSurface dungeon)
    {
        _dungeon = dungeon;
    }

    public void Process(NewTurnEvent eventData)
    {
        foreach (EcsEntity enemy in EcsApp.EnemiesGroup)
        {
            var type = enemy.GetComponent<EnemyComponent>().MonsterType;
            switch (type)
            {
                default:
                    DefaultMonsterMove(enemy);
                    break;
            }
        }
    }

    private void DefaultMonsterMove(EcsEntity enemy)
    {
        var posCmp = enemy.GetComponent<PositionComponent>();
        var pos = posCmp.Position.Value;
        var moves = pos.
            Neighbors(GameSettings.DungeonWidth, GameSettings.DungeonHeight, false).
            Where(MapGenerator.IsWalkable).
            ToArray();
        posCmp.Position.Value = moves[_rng.NextInt(moves.Length)];
    }
}
