using EcsRx.Extensions;
using Roguish.Serialization;
// ReSharper disable IdentifierTypo


namespace Roguish.ECS.Components;
public class EquippedComponent : EcsComponent
{
    // TODO: Think about making access routines so the following comment is unnecessary...
    // For instance, one routine that returns all names of the slots and another which
    // takes a name and returns or sets the value of the corresponding slot.  Perhaps one
    // which returns names of filled slots and their values 
    
    // WARNING: this needs to track the code in UnequipDialog() and
    // InventorySurface.Equip()! 
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

    internal void Clear()
    {
        Headgear = -1;
        Footwear = -1;
        Chest = -1;
        LRing = -1;
        RRing = -1;
        WeaponLeft = -1;
        WeaponRight = -1;
        Arms = -1;
        Legs = -1;
        Amulet = -1;
        Gloves = -1;
        Belt = -1;
    }

    internal record FilledSlot(string name, int id);
    internal List<FilledSlot> GetFilledSlots()
    {
        var filledSlots = new List<FilledSlot>();
        if (Headgear >= 0)
        {
            filledSlots.Add(new FilledSlot("Headgear", Headgear));
        }
        if (Footwear >= 0)
        {
            filledSlots.Add(new FilledSlot("Footwear", Footwear));
        }
        if (Chest >= 0)
        {
            filledSlots.Add(new FilledSlot("Chest", Chest));
        }
        if (LRing >= 0)
        {
            filledSlots.Add(new FilledSlot("Left Ring", LRing));
        }
        if (RRing >= 0)
        {
            filledSlots.Add(new FilledSlot("Right Ring", RRing));
        }

        if (WeaponLeft == WeaponRight && WeaponLeft >= 0)
        {
            filledSlots.Add(new FilledSlot("Two Handed", WeaponLeft));
        }
        else
        {
            if (WeaponLeft >= 0)
            {
                filledSlots.Add(new FilledSlot("Left Hand", WeaponLeft));
            }
            if (WeaponRight >= 0)
            {
                filledSlots.Add(new FilledSlot("Right Hand", WeaponRight));
            }
        }
        if (Arms >= 0)
        {
            filledSlots.Add(new FilledSlot("Arms", Arms));
        }
        if (Legs >= 0)
        {
            filledSlots.Add(new FilledSlot("Legs", Legs));
        }
        if (Amulet >= 0)
        {
            filledSlots.Add(new FilledSlot("Amulet", Amulet));
        }
        if (Gloves >= 0)
        {
            filledSlots.Add(new FilledSlot("Gloves", Gloves));
        }
        if (Belt >= 0)
        {
            filledSlots.Add(new FilledSlot("Belt", Belt));
        }
        return filledSlots;
    }

    internal void UnequipSlot(FilledSlot slot)
    {
        switch (slot.name)
        {
            case "Left Hand":
                WeaponLeft = -1;
                break;

            case "Right Hand":
                WeaponRight = -1;
                break;

            case "Headgear":
                Headgear = -1;
                break;

            case "Footwear":
                Footwear = -1;
                break;

            case "Chest":
                Chest = -1;
                break;

            case "Left Ring":
                LRing = -1;
                break;

            case "Right Ring":
                RRing = -1;
                break;

            case "Arms":
                Arms = -1;
                break;

            case "Legs":
                Legs = -1;
                break;

            case "Amulet":
                Amulet = -1;
                break;

            case "Gloves":
                Gloves = -1;
                break;

            case "Belt":
                Belt = -1;
                break;

            case "Two Handed":
                WeaponLeft = -1;
                WeaponRight = -1;
                break;
        }
    }
}
