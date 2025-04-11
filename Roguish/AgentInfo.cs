using System.Diagnostics;
using EcsRx.Blueprints;
using GoRogue.Random;
using Roguish.ECS.Components;
using ShaiRandom.Generators;
using EcsRx.Extensions;
using Newtonsoft.Json;
using Roguish.Screens;
using Roguish.ECS.Tasks;

namespace Roguish;

#region Enums
internal enum AgentType
{
    Player,
    Rat,
    Goblin,
    Orc,
    Troll,
    Dragon
};
#endregion

internal class AgentInfo
{
    #region Public Properties
    public string Name { get; set; } = "UnnamedAgent";
    public string Description { get; set; } = "An agent";
    public int StartLevel { get; set; }
    public int EndLevel { get; set; }
    public int Glyph { get; set; }
    public Color Color { get; set; } = Color.White;
    public int HealthMin { get; set; } = 1;
    public int HealthMax { get; set; } = 1;
    public ulong Move { get; set; } = 100;

    public AgentType AgentType { get; set; } = AgentType.Player;
    #endregion

    #region Private Fields
    private static readonly Dictionary<int, List<AgentInfo>> MpLevelToAgents = new();
    private static readonly Dictionary<AgentType, AgentInfo> MpTypeToInfo = new();
    private static readonly IEnhancedRandom Rng = GlobalRandom.DefaultRNG;
    #endregion

    #region Static Constructor
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
    #endregion

    #region Queries
    private static AgentInfo PickAgentForLevel(int iLevel)
    {
        var available = MpLevelToAgents[iLevel];
        return available[Rng.NextInt(available.Count)];
    }

    internal static AgentInfo InfoFromType(AgentType type) => MpTypeToInfo[type];
    #endregion

    #region Blueprints
    internal static IBlueprint GetPlayerBlueprint(int maxHealth, DungeonSurface dungeon)
    {
        var pos = new Point(0, 0);
        var scEntity = dungeon.GetPlayerScEntity(pos);
        return new PlayerBlueprint
        {
            AgentType = AgentType.Player,
            Name = "Player",
            Description = "It's you silly!",
            MaxHealth = maxHealth,
            ScEntity = scEntity,
            Move = TaskGetter.StdMovementTime,
            // First player task is reserved for movement
            Task = new ( null!, TaskGetter.CreateRegenerateTask(300))
        };
    }

    internal static IBlueprint GetBlueprint(int iLevel, DungeonSurface dungeon)
    {
        var info = PickAgentForLevel(iLevel);
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
            Task = task,
            Move = info.Move
        };
    }

    internal class BaseBlueprint : IBlueprint
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int MaxHealth { get; set; }
        public required ScEntity ScEntity { get; set; }
        public required AgentType AgentType { get; set; }
        public required TaskComponent? Task { get; set; }
        public required ulong Move { get; set; }

        public void Apply(EcsEntity entity)
        {
            entity.AddComponent(new DescriptionComponent(Name, Description));
            entity.AddComponent(new HealthComponent(MaxHealth));
            entity.AddComponent(new DisplayComponent(ScEntity));
            entity.AddComponent(new LevelItemComponent(CurrentLevel));
            entity.AddComponent(new PositionComponent(ScEntity.Position));
            if (Task != null)
            {
                entity.AddComponent(Task);
            }
        }
    }

    internal class AgentBlueprint : BaseBlueprint, IBlueprint
    {
        public new void Apply(EcsEntity entity)
        {
            entity.AddComponent(new AgentComponent(AgentType, Move));
            // Start with a random fireon time to stagger agent moves randomly
            entity.AddComponent(new TaskComponent(TaskGetter.Ticks + Move * Rng.NextULong(100) / 100ul, 
                TaskType.AgentMove));
            entity.AddComponent(new EntityTypeComponent(EcsType.Agent));
            // base Apply does position which calls movement system which requires EntityType
            // so it has to come after the EntityTypeComponent add in this routine
            base.Apply(entity);
        }
    }

    internal class PlayerBlueprint : BaseBlueprint, IBlueprint
    {
        public new void Apply(EcsEntity entity)
        {
            entity.AddComponent(new EntityTypeComponent(EcsType.Player));
            entity.AddComponent<IsPlayerControlledComponent>();
            entity.AddComponent<EquippedComponent>();
            // base Apply does position which calls movement system which requires EntityType
            // so it has to come after the EntityTypeComponent add in this routine
            base.Apply(entity);
        }
    }
    #endregion
}
