using GoRogue.Random;
using Newtonsoft.Json;
using Roguish.ECS.Components;
using Roguish.Screens;
using ShaiRandom.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsRx.Blueprints;
using EcsRx.Extensions;

namespace Roguish.Info;

internal enum ArmorType
{
    Chest,
    Gloves,
    Headgear,
    Footwear,
    Belt,
    Arms,
}

internal class ArmorInfo
{
    #region Public properties
    public ArmorType ArmorType { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EquipSlots Slot { get; set; }
    public int StartLevel { get; set; }
    public int EndLevel { get; set; }
    public int Glyph { get; set; }
    public Color Color { get; set; }
    public int ArmorCount { get; set; }
    #endregion

    #region Private fields
    private static readonly Dictionary<ArmorType, ArmorInfo> MpTypeToInfo = new();
    private static readonly Dictionary<int, List<ArmorInfo>> MpLevelToArmor = new();
    private static readonly IEnhancedRandom Rng = GlobalRandom.DefaultRNG;
    #endregion

    #region Constructors
    static ArmorInfo()
    {
        var jsonArmor = File.ReadAllText("JSON/Armor.json");
        var armorList = JsonConvert.DeserializeObject<List<ArmorInfo>>(jsonArmor);

        Debug.Assert(armorList != null, nameof(armorList) + " != null");
        foreach (var armorInfo in armorList)
        {
            MpTypeToInfo[armorInfo.ArmorType] = armorInfo;
            for (var i = armorInfo.StartLevel; i <= armorInfo.EndLevel; i++)
            {
                if (!MpLevelToArmor.ContainsKey(i))
                {
                    MpLevelToArmor[i] = [];
                }
                MpLevelToArmor[i].Add(armorInfo);
            }
        }
    }
    #endregion

    #region Queries
    internal static ArmorInfo PickArmorForLevel(int iLevel)
    {
        var available = MpLevelToArmor[iLevel];
        return available[Rng.NextInt(available.Count)];
    }

    internal static ArmorInfo InfoFromType(ArmorType type) => MpTypeToInfo[type];
    #endregion

    #region Blueprints
    internal static IBlueprint GetBlueprint(int iLevel, DungeonSurface dungeon)
    {
        var info = PickArmorForLevel(iLevel);
        var pos = dungeon.Mapgen.FindRandomEmptyPoint();
        var scEntity = dungeon.CreateScEntity(info.Color, pos, info.Glyph, 0);

        return new ArmorBlueprint
        {
            Name = info.Name,
            Description = info.Description,
            ScEntity = scEntity,
            ArmorType = info.ArmorType,
            Slot = info.Slot,
        };
    }


    internal class ArmorBlueprint : IBlueprint
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required ScEntity ScEntity { get; set; }
        public required ArmorType ArmorType { get; set; }
        public required EquipSlots Slot { get; set; }

        public void Apply(EcsEntity entity)
        {
            entity.AddComponent(new DescriptionComponent(Name, Description));
            entity.AddComponent(new DisplayComponent(ScEntity));
            entity.AddComponent(new LevelItemComponent(CurrentLevel));
            entity.AddComponent(new EquipableComponent(Slot));
            entity.AddComponent(new EntityTypeComponent(EcsType.Armor));
            // Ensure that position is added AFTER EntityType because the move system requires EntityType
            entity.AddComponent(new PositionComponent(ScEntity.Position));
            entity.AddComponent(new ArmorTypeComponent(ArmorType));
        }
    }
    #endregion
}
