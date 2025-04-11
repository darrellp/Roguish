using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguish.Serialization;


namespace Roguish.ECS.Components;
public class EquippedComponent : EcsComponent
{
    public int Headgear { get; set; } = -1;
    public int Footwear { get; set; } = -1;
    public int Chest { get; set; } = -1;
    public int LRing { get; set; } = -1;
    public int RRing { get; set; } = -1;
    public int WeaponLeft { get; set; } = -1;
    public int WeaponRight { get; set; } = -1;
    public int Arms { get; set; } = -1;
    public int Legs { get; set; } = -1;
    public int Amulet { get; set; } = -1;
    public int Gloves { get; set; } = -1;
    public int Belt { get; set; } = -1;

    internal void RemapEquipment(Dictionary<int, int> mpOldIdToNewId)
    {
        Headgear = Serialize.RemapId(Headgear, mpOldIdToNewId);
        Footwear = Serialize.RemapId(Footwear, mpOldIdToNewId);
        Chest = Serialize.RemapId(Chest, mpOldIdToNewId);
        LRing = Serialize.RemapId(LRing, mpOldIdToNewId);
        RRing = Serialize.RemapId(RRing, mpOldIdToNewId);
        WeaponLeft = Serialize.RemapId(WeaponLeft, mpOldIdToNewId);
        WeaponRight = Serialize.RemapId(WeaponRight, mpOldIdToNewId);
        Arms = Serialize.RemapId(Arms, mpOldIdToNewId);
        Legs = Serialize.RemapId(Legs, mpOldIdToNewId);
        Amulet = Serialize.RemapId(Amulet, mpOldIdToNewId);
        Gloves = Serialize.RemapId(Gloves, mpOldIdToNewId);
        Belt = Serialize.RemapId(Belt, mpOldIdToNewId);
    }

}
