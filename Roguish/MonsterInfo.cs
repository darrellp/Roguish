using System.Diagnostics;
using EcsRx.Blueprints;
using GoRogue.Random;
using Roguish.ECS.Components;
using ShaiRandom.Generators;
using EcsRx.Extensions;
using Newtonsoft.Json;
using Roguish.ECS.Systems;

namespace Roguish;

public enum MonsterType
{
    Player,
    Rat,
    Goblin,
    Orc,
    Troll,
    Dragon
};

internal class MonsterInfo
{
    public string Name { get; set; } = "UnnamedMonster";
    public string Description { get; set; } = "A monster";
    public int StartLevel { get; set; }
    public int EndLevel { get; set; }
    public int Glyph { get; set; }
    public Color Color { get; set; } = Color.White;
    public int HealthMin { get; set; } = 1;
    public int HealthMax { get; set; } = 1;
    public MonsterType MonsterType { get; set; } = MonsterType.Player;

    private static readonly IEnhancedRandom Rng = GlobalRandom.DefaultRNG;

    public static IBlueprint GetPlayerBlueprint(int maxHealth, DungeonSurface dungeon)
    {
        var pos = new Point(0, 0);
        var scEntity = dungeon.CreateScEntity(Color.White, pos, 2, 100);
        return new PlayerBlueprint
        {
            MonsterType = MonsterType.Player,
            Name = "Player",
            Description = "It's you silly!",
            MaxHealth = maxHealth,
            ScEntity = scEntity,
            Task = null
        };
    }

    private static readonly Dictionary<int, List<MonsterInfo>> MpLevelToMonsters = new();

   static MonsterInfo()
    {
        var jsonMonsters = File.ReadAllText("JSON/monsters.json");
        var monsterList = JsonConvert.DeserializeObject<List<MonsterInfo>>(jsonMonsters);

        Debug.Assert(monsterList != null, nameof(monsterList) + " != null");
        foreach (var monsterInfo in monsterList)
        {
            for (var i = monsterInfo.StartLevel; i <= monsterInfo.EndLevel; i++)
            {
                if (!MpLevelToMonsters.ContainsKey(i))
                {
                    MpLevelToMonsters[i] = [];
                }
                MpLevelToMonsters[i].Add(monsterInfo);
            }
        }
    }

    private static MonsterInfo PickMonsterForLevel(int iLevel)
    {
        var available = MpLevelToMonsters[iLevel];
        return available[Rng.NextInt(available.Count)];
    }

    public static IBlueprint GetBlueprint(int iLevel, DungeonSurface dungeon)
    {
        var info = PickMonsterForLevel(iLevel);
        var maxHealth = Rng.NextInt(info.HealthMin, info.HealthMax + 1);
        var pos = dungeon.Mapgen.FindRandomEmptyPoint();
        var scEntity = dungeon.CreateScEntity(info.Color, pos, info.Glyph, 0);
        TaskComponent? task = null;
        return new MonsterBlueprint
        {
            Name = info.Name,
            Description = info.Description,
            MaxHealth = maxHealth,
            ScEntity = scEntity,
            MonsterType = info.MonsterType,
            Task = task
        };
    }

    public class PlayerBlueprint : IBlueprint
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int MaxHealth { get; set; }
        public required ScEntity ScEntity { get; set; }
        public required MonsterType MonsterType { get; set; }
        public required TaskComponent? Task { get; set; }

        public void Apply(EcsEntity entity)
        {
            entity.AddComponent(new DescriptionComponent(Name, Description));
            entity.AddComponent(new HealthComponent(MaxHealth));
            entity.AddComponent(new DisplayComponent(ScEntity));
            entity.AddComponent(new LevelItemComponent());
            entity.AddComponent(new PositionComponent(ScEntity.Position));
        }
    }

    public class MonsterBlueprint : PlayerBlueprint, IBlueprint
    {
        public new void Apply(EcsEntity entity)
        {
            const int monsterMoveTime = 100;
            base.Apply(entity);
            entity.AddComponent(new AgentComponent(MonsterType, monsterMoveTime));
            entity.AddComponent(new TaskComponent(monsterMoveTime, NewTurnEventSystem.DefaultMonsterMove));
        }
    }
}
