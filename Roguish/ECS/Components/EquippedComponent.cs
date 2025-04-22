using EcsRx.Extensions;
using Roguish.Info;
using Roguish.Serialization;
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
        set { _headgear = value; CalcAc(); }
    }

    public int Footwear
    {
        get => _footwear;
        set { _footwear = value; CalcAc(); }
    }

    public int Chest
    {
        get => _chest;
        set { _chest = value; CalcAc(); }
    }

    public int LRing { get; set; } = -1;
    public int RRing { get; set; } = -1;
    public int WeaponLeft { get; set; } = -1;
    public int WeaponRight { get; set; } = -1;

    public int Arms
    {
        get => _arms;
        set { _arms = value; CalcAc(); }
    }

    public int Legs
    {
        get => _legs;
        set { _legs = value; CalcAc(); }
    }

    public int Amulet { get; set; } = -1;

    public int Gloves
    {
        get => _gloves;
        set { _gloves = value; CalcAc(); }
    }

    public int Belt
    {
        get => _belt;
        set { _belt = value; CalcAc(); }
    }
    #endregion

    #region Miscellaneous
    public void CalcAc()
    {
        ArmorCount = 0;
        foreach (var slot in GetFilledSlots())
        {
            var entity = EcsApp.EntityDatabase.GetEntity(slot.Id);
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

    internal void UnequipSlot(FilledSlot slot)
    {
        slot.SetId(-1);
    }
    #endregion

    #region GetFilledSlots
    internal record FilledSlot(string Name, int Id, Action<int> SetId);
    internal List<FilledSlot> GetFilledSlots()
    {
        var filledSlots = new List<FilledSlot>();
        if (Headgear >= 0)
        {
            filledSlots.Add(new FilledSlot("Headgear", Headgear, i => Headgear = i));
        }
        if (Footwear >= 0)
        {
            filledSlots.Add(new FilledSlot("Footwear", Footwear, i => Footwear = i));
        }
        if (Chest >= 0)
        {
            filledSlots.Add(new FilledSlot("Chest", Chest, i => Chest = i));
        }
        if (LRing >= 0)
        {
            filledSlots.Add(new FilledSlot("Left Ring", LRing, i => LRing = i));
        }
        if (RRing >= 0)
        {
            filledSlots.Add(new FilledSlot("Right Ring", RRing, i => RRing = i));
        }

        if (WeaponLeft == WeaponRight && WeaponLeft >= 0)
        {
            filledSlots.Add(new FilledSlot("Two Handed", WeaponLeft, i =>
            {
                WeaponLeft = i;
                WeaponRight = i;
            }));
        }
        else
        {
            if (WeaponLeft >= 0)
            {
                filledSlots.Add(new FilledSlot("Left Hand", WeaponLeft, i => WeaponLeft = i));
            }
            if (WeaponRight >= 0)
            {
                filledSlots.Add(new FilledSlot("Right Hand", WeaponRight, i => WeaponRight = i));
            }
        }
        if (Arms >= 0)
        {
            filledSlots.Add(new FilledSlot("Arms", Arms, i => Arms = i));
        }
        if (Legs >= 0)
        {
            filledSlots.Add(new FilledSlot("Legs", Legs, i => Legs = i));
        }
        if (Amulet >= 0)
        {
            filledSlots.Add(new FilledSlot("Amulet", Amulet, i => Amulet = i));
        }
        if (Gloves >= 0)
        {
            filledSlots.Add(new FilledSlot("Gloves", Gloves, i => Gloves = i));
        }
        if (Belt >= 0)
        {
            filledSlots.Add(new FilledSlot("Belt", Belt, i => Belt = i));
        }
        return filledSlots;
    }
    #endregion
}
