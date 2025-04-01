using System.Diagnostics;
using EcsR3.Blueprints;
using EcsR3.Entities;
using GoRogue.Random;
using Newtonsoft.Json;
using ShaiRandom.Generators;

namespace Roguish;
using ECS.Components;

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
    public Color Color { get; set; } = Color.White;
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

    internal class WeaponBlueprint : IBlueprint
    {
        public void Apply(IEntity entity)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}