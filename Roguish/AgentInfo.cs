using System.Diagnostics;
using EcsRx.Blueprints;
using GoRogue.Random;
using Roguish.ECS.Components;
using ShaiRandom.Generators;
using EcsRx.Extensions;
using Newtonsoft.Json;
using Roguish.ECS.Systems;
using Roguish.Screens;

namespace Roguish;

public enum AgentType
{
    Player,
    Rat,
    Goblin,
    Orc,
    Troll,
    Dragon
};

internal class AgentInfo
{
    public string Name { get; set; } = "UnnamedAgent";
    public string Description { get; set; } = "An agent";
    public int StartLevel { get; set; }
    public int EndLevel { get; set; }
    public int Glyph { get; set; }
    public Color Color { get; set; } = Color.White;
    public int HealthMin { get; set; } = 1;
    public int HealthMax { get; set; } = 1;
    public AgentType AgentType { get; set; } = AgentType.Player;

    private static readonly Dictionary<int, List<AgentInfo>> MpLevelToAgents = new();
    private static readonly Dictionary<AgentType, AgentInfo> MpTypeToInfo = new();

    private static readonly IEnhancedRandom Rng = GlobalRandom.DefaultRNG;

    public static IBlueprint GetPlayerBlueprint(int maxHealth, DungeonSurface dungeon)
    {
        var pos = new Point(0, 0);
        var scEntity = dungeon.CreateScEntity(Color.White, pos, 2, 100);
        return new PlayerBlueprint
        {
            AgentType = AgentType.Player,
            Name = "Player",
            Description = "It's you silly!",
            MaxHealth = maxHealth,
            ScEntity = scEntity,
            Task = null
        };
    }

   static AgentInfo()
    {
        var jsonMonsters = File.ReadAllText("JSON/monsters.json");
        var agentList = JsonConvert.DeserializeObject<List<AgentInfo>>(jsonMonsters);

        Debug.Assert(agentList != null, nameof(agentList) + " != null");
        foreach (var agentInfo in agentList)
        {
            MpTypeToInfo[agentInfo.AgentType] = agentInfo;
            for (var i = agentInfo.StartLevel; i <= agentInfo.EndLevel; i++)
            {
                if (!MpLevelToAgents.ContainsKey(i))
                {
                    MpLevelToAgents[i] = [];
                }
                MpLevelToAgents[i].Add(agentInfo);
            }
        }
    }

    private static AgentInfo PickMonsterForLevel(int iLevel)
    {
        var available = MpLevelToAgents[iLevel];
        return available[Rng.NextInt(available.Count)];
    }

    public static AgentInfo InfoFromType(AgentType type) => MpTypeToInfo[type];

    public static IBlueprint GetBlueprint(int iLevel, DungeonSurface dungeon)
    {
        var info = PickMonsterForLevel(iLevel);
        var maxHealth = Rng.NextInt(info.HealthMin, info.HealthMax + 1);
        var pos = dungeon.Mapgen.FindRandomEmptyPoint();
        var scEntity = dungeon.CreateScEntity(info.Color, pos, info.Glyph, 0);
        TaskComponent? task = null;
        return new AgentBlueprint
        {
            Name = info.Name,
            Description = info.Description,
            MaxHealth = maxHealth,
            ScEntity = scEntity,
            AgentType = info.AgentType,
            Task = task
        };
    }

    public class PlayerBlueprint : IBlueprint
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int MaxHealth { get; set; }
        public required ScEntity ScEntity { get; set; }
        public required AgentType AgentType { get; set; }
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

    public class AgentBlueprint : PlayerBlueprint, IBlueprint
    {
        public new void Apply(EcsEntity entity)
        {
            const int agentMoveTime = 100;
            base.Apply(entity);
            entity.AddComponent(new AgentComponent(AgentType, agentMoveTime));
            entity.AddComponent(new TaskComponent(agentMoveTime, NewTurnEventSystem.DefaultAgentMove));
        }
    }
}
