using EcsRx.Blueprints;
using GoRogue.Random;
using Roguish.ECS.Components;
using ShaiRandom.Generators;
using EcsRx.Extensions;

namespace Roguish;

enum MonsterType
{
    Rat,
    Goblin,
    Orc,
    Troll,
    Dragon
};

internal record MonsterInfo
{
    private string Name { get; init; } = "UnnamedMonster";
    private string Description { get; init; } = "A monster";
    private int StartLevel { get; init; } = 0;
    private int EndLevel { get; init; } = 0;
    private int Glyph { get; init; } = 0;
    private Color Color { get; init; } = Color.White;
    private int HealthMin { get; init; } = 1;
    private int HealthMax { get; init; } = 1;

    private static readonly IEnhancedRandom _rng = GlobalRandom.DefaultRNG;

    private static readonly List<MonsterInfo> MonsterList = [
        new MonsterInfo(MonsterType.Rat,
            "Rat", "Furry little rodent",
            10, 15,
            0, 3, 
            'r', 
            Color.Yellow),
        new MonsterInfo(MonsterType.Goblin, 
            "Goblin", "Ugly green bugger",
            10, 15,
            2, 5, 
            'g', 
            Color.Green),
        new MonsterInfo(MonsterType.Orc, 
            "Orc", "Something from your nightmares",
            10, 15,
            4, 7, 
            'o', 
            Color.Red),
        new MonsterInfo(MonsterType.Troll, 
            "Troll", "Big, strong and dumb",
            10, 15,
            6, 9, 
            't', 
            Color.DarkGreen),
        new MonsterInfo(MonsterType.Dragon, 
            "Dragon", "Scaly fire breathing lizard",
            10, 15,
            8, 10, 
            'd', 
            Color.DarkRed)
    ];

    public static IBlueprint PlayerBlueprint(int maxHealth, DungeonSurface dungeon)
    {
        var pos = new Point(0, 0);
        var scEntity = dungeon.CreateScEntity(Color.White, pos, '@', 100);
        return new MonsterBlueprint
        {
            Name = "Player",
            Description = "It's you silly!",
            MaxHealth = maxHealth,
            ScEntity = scEntity
        };
    }

    private static readonly Dictionary<int, List<MonsterInfo>> MpLevelToMonsters = new();

   static MonsterInfo()
    {
        foreach (var monsterInfo in MonsterList)
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

    public MonsterInfo(
        MonsterType type, 
        string name, string description,
        int healthMin, int healthMax,
        int startLevel, int endLevel, 
        int glyph, 
        Color color)
    {
        Name = name;
        StartLevel = startLevel;
        EndLevel = endLevel;
        Glyph = glyph;
        Color = color;
        Description = description;
        HealthMin = healthMin;
        HealthMax = healthMax;
    }

    private static MonsterInfo PickMonsterForLevel(int iLevel)
    {
        var available = MpLevelToMonsters[iLevel];
        return available[_rng.NextInt(available.Count)];
    }

    public static IBlueprint GetBlueprint(int iLevel, DungeonSurface dungeon)
    {
        var info = PickMonsterForLevel(iLevel);
        var maxHealth = _rng.NextInt(info.HealthMin, info.HealthMax + 1);
        var pos = dungeon.FindRandomEmptyPoint();
        var scEntity = dungeon.CreateScEntity(info.Color, pos, info.Glyph, 0);
        return new MonsterBlueprint
        {
            Name = info.Name,
            Description = info.Description,
            MaxHealth = maxHealth,
            ScEntity = scEntity
        };
    }

    public class MonsterBlueprint : IBlueprint
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int MaxHealth { get; set; }
        public required ScEntity ScEntity { get; set; }

        public void Apply(EcsEntity entity)
        {
            entity.AddComponent(new DescriptionComponent( Name, Description));
            entity.AddComponent(new HealthComponent( MaxHealth));
            entity.AddComponent(new DisplayComponent(ScEntity));
            entity.AddComponent(new LevelItemComponent());
            entity.AddComponent(new PositionComponent(ScEntity.Position));
        }
    }
}
