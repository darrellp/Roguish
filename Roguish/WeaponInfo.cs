using System.Diagnostics;
using EcsRx.Blueprints;
using EcsRx.Extensions;
using GoRogue.Random;
using Newtonsoft.Json;
using ShaiRandom.Generators;

namespace Roguish;
using ECS.Components;
using Roguish.Screens;
using static Roguish.AgentInfo;

internal enum WeaponType
{
    Axe,
    Sword,
    Staff,
    Club,
}
internal class WeaponInfo
{
    #region Public properties
    public WeaponType WeaponType { get; set; }
    public string Name {get; set;}
    public string Description {get; set;}
    public EquipSlots Slot { get; set; }
    public int StartLevel { get; set; }
    public int EndLevel { get; set; }
    public int Glyph { get; set; }
    public Color Color { get; set; }
    #endregion
    
    #region Private fields
    private static Dictionary<WeaponType, WeaponInfo> MpTypeToInfo = new();
    private static Dictionary<int, List<WeaponInfo>> MpLevelToWeapons = new();
    private static readonly IEnhancedRandom Rng = GlobalRandom.DefaultRNG;
    #endregion
    
    #region Constructors
    static WeaponInfo()
    {
        var jsonWeapons = File.ReadAllText("JSON/weapons.json");
        var weaponsList = JsonConvert.DeserializeObject<List<WeaponInfo>>(jsonWeapons);

        Debug.Assert(weaponsList != null, nameof(weaponsList) + " != null");
        foreach (var weaponInfo in weaponsList)
        {
            MpTypeToInfo[weaponInfo.WeaponType] = weaponInfo;
            for (var i = weaponInfo.StartLevel; i <= weaponInfo.EndLevel; i++)
            {
                if (!MpLevelToWeapons.ContainsKey(i))
                {
                    MpLevelToWeapons[i] = [];
                }
                MpLevelToWeapons[i].Add(weaponInfo);
            }
        }
    }
    #endregion
    
    #region Queries
    internal static WeaponInfo PickWeaponForLevel(int iLevel)
    {
        var available = MpLevelToWeapons[iLevel];
        return available[Rng.NextInt(available.Count)];
    }

    internal static WeaponInfo InfoFromType(WeaponType type) => MpTypeToInfo[type];
    #endregion

    #region Blueprints
    internal static IBlueprint GetBlueprint(int iLevel, DungeonSurface dungeon)
    {
        var info = PickWeaponForLevel(iLevel);
        var pos = dungeon.Mapgen.FindRandomEmptyPoint();
        var scEntity = dungeon.CreateScEntity(info.Color, pos, info.Glyph, 0);

        return new WeaponBlueprint
        {
            Name = info.Name,
            Description = info.Description,
            ScEntity = scEntity,
            WeaponType = info.WeaponType,
            Slot = info.Slot,
        };
    }


    internal class WeaponBlueprint : IBlueprint
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required ScEntity ScEntity { get; set; }
        public required WeaponType WeaponType { get; set; }
        public required EquipSlots Slot { get; set; }

        public void Apply(EcsEntity entity)
        {
            entity.AddComponent(new DescriptionComponent(Name, Description));
            entity.AddComponent(new DisplayComponent(ScEntity));
            entity.AddComponent(new LevelItemComponent());
            entity.AddComponent(new PositionComponent(ScEntity.Position));
            entity.AddComponent(new EquipableComponent(Slot));
            entity.AddComponent(new EntityTypeComponent(EcsType.Weapon));
        }
    }
    #endregion
}