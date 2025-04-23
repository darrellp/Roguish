using EcsRx.Extensions;
using Roguish.Info;
using Roguish.Serialization;
using System.Security.Cryptography;
// ReSharper disable IdentifierTypo


namespace Roguish.ECS.Components;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   An equipped component. </summary>
///
/// <remarks>   This is WAY fatter than I'd like to make it but it's pretty much devoted to either
///             readonly queries or recalculating the armor count.  I want to keep most of this stuff
///             in here so that if new equip slots are added in there is one place where changes to
///             accommodate the new slots properly.
///             Darrell Plank, 4/21/2025. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class EquippedComponent : EcsComponent
{
    #region Private Fields
    private int _headgear = -1;
    private int _footwear = -1;
    private int _chest = -1;
    private int _arms = -1;
    private int _legs = -1;
    private int _gloves = -1;
    private int _belt = -1;
    #endregion

    #region Public Properties
    public int ArmorCount { get; set; }
    #endregion

    #region Equipment
    // TODO: Think about making access routines so the following comment is unnecessary...
    // For instance, one routine that returns all names of the slots and another which
    // takes a name and returns or sets the value of the corresponding slot.  Perhaps one
    // which returns names of filled slots and their values 

    // WARNING: this needs to track the code in InventorySurface.Equip()! 
    public int Headgear
    {
        get => _headgear;
        set { _headgear = value; CalcArmourCount(); }
    }

    public int Footwear
    {
        get => _footwear;
        set { _footwear = value; CalcArmourCount(); }
    }

    public int Chest
    {
        get => _chest;
        set { _chest = value; CalcArmourCount(); }
    }

    public int LRing { get; set; } = -1;
    public int RRing { get; set; } = -1;
    public int WeaponLeft { get; set; } = -1;
    public int WeaponRight { get; set; } = -1;

    public int Arms
    {
        get => _arms;
        set { _arms = value; CalcArmourCount(); }
    }

    public int Legs
    {
        get => _legs;
        set { _legs = value; CalcArmourCount(); }
    }

    public int Amulet { get; set; } = -1;

    public int Gloves
    {
        get => _gloves;
        set { _gloves = value; CalcArmourCount(); }
    }

    public int Belt
    {
        get => _belt;
        set { _belt = value; CalcArmourCount(); }
    }
    #endregion

    #region Miscellaneous
    public void CalcArmourCount()
    {
        ArmorCount = 0;
        foreach (var slot in GetFilledSlots())
        {
            var entity = EcsApp.EntityDatabase.GetEntity(slot.Id);
            if (entity == null)
            {
                // This happens in deserialization
                return;
            }
            if (entity.HasComponent<ArmorTypeComponent>())
            {
                var type = entity.GetComponent<ArmorTypeComponent>();
                var info = ArmorInfo.InfoFromType(type.ArmorType);
                ArmorCount += info.ArmorCount;
            }
        }
    }

    internal void RemapEquipment(Dictionary<int, int> mpOldIdToNewId)
    {
        foreach (var slot in GetFilledSlots())
        {
            slot.SetId(Serialize.RemapId(slot.Id, mpOldIdToNewId));
        }
    }

    internal void Clear()
    {
        foreach (var slot in GetFilledSlots())
        {
            slot.SetId(-1);
        }
    }

    internal void UnequipSlot(SlotInfo slotInfo)
    {
        slotInfo.SetId(-1);
    }
    #endregion

    #region GetFilledSlots
    internal record SlotInfo(string Name, int Id, Action<int> SetId);
    internal List<SlotInfo> GetFilledSlots(bool getAll = false)
    {
        var filledSlots = new List<SlotInfo>();
        if (getAll || Headgear >= 0)
        {
            filledSlots.Add(new SlotInfo("Headgear", Headgear, i => Headgear = i));
        }
        if (getAll || Footwear >= 0)
        {
            filledSlots.Add(new SlotInfo("Footwear", Footwear, i => Footwear = i));
        }
        if (getAll || Chest >= 0)
        {
            filledSlots.Add(new SlotInfo("Chest", Chest, i => Chest = i));
        }
        if (getAll || LRing >= 0)
        {
            filledSlots.Add(new SlotInfo("Left Ring", LRing, i => LRing = i));
        }
        if (getAll || RRing >= 0)
        {
            filledSlots.Add(new SlotInfo("Right Ring", RRing, i => RRing = i));
        }

        if (getAll)
        {
            filledSlots.Add(new SlotInfo("Two Handed", WeaponLeft, i =>
            {
                WeaponLeft = i;
                WeaponRight = i;
            }));
            filledSlots.Add(new SlotInfo("Left Hand", WeaponLeft, i => WeaponLeft = i));
            filledSlots.Add(new SlotInfo("Right Hand", WeaponRight, i => WeaponRight = i));
        }
        else
        {
            if (WeaponLeft == WeaponRight && WeaponLeft >= 0)
            {
                filledSlots.Add(new SlotInfo("Two Handed", WeaponLeft, i =>
                {
                    WeaponLeft = i;
                    WeaponRight = i;
                }));
            }
            else
            {
                if (WeaponLeft >= 0)
                {
                    filledSlots.Add(new SlotInfo("Left Hand", WeaponLeft, i => WeaponLeft = i));
                }
                if (WeaponRight >= 0)
                {
                    filledSlots.Add(new SlotInfo("Right Hand", WeaponRight, i => WeaponRight = i));
                }
        }
        }
        if (getAll || Arms >= 0)
        {
            filledSlots.Add(new SlotInfo("Arms", Arms, i => Arms = i));
        }
        if (getAll || Legs >= 0)
        {
            filledSlots.Add(new SlotInfo("Legs", Legs, i => Legs = i));
        }
        if (getAll || Amulet >= 0)
        {
            filledSlots.Add(new SlotInfo("Amulet", Amulet, i => Amulet = i));
        }
        if (getAll || Gloves >= 0)
        {
            filledSlots.Add(new SlotInfo("Gloves", Gloves, i => Gloves = i));
        }
        if (getAll || Belt >= 0)
        {
            filledSlots.Add(new SlotInfo("Belt", Belt, i => Belt = i));
        }
        return filledSlots;
    }

    internal SlotInfo AvailableSlotFromSlotType(EquipSlots equipSlot)
    {
        var availableSlotName = (equipSlot switch
        {
            EquipSlots.TwoHands => "Two Handed",
            EquipSlots.OneHand when WeaponRight >= 0 || WeaponLeft < 0 => "Left Hand",
            EquipSlots.OneHand => "Right Hand",
            EquipSlots.Ring when RRing >= 0 || LRing < 0 => "LRing",
            EquipSlots.Ring => "RRing",
            _ => Enum.GetName(equipSlot)
        })!;
        return GetFilledSlots(true).First(s => s.Name == availableSlotName);
    }
    #endregion
}
