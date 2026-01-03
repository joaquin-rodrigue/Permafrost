using UnityEngine;
/// <summary>
///     Enumerator for the various item types. Item types themselves are stored as an integer,
///     and this enum corresponds the various bits of an int to different item types. If you're
///     trying to determine if an item is of a certain type, you should use the <c>IsType()</c>
///     method on the Item object for simplicity.
/// </summary>
public enum ItemType
{
    Food =     0b0000000000000001,
    Weapon =   0b0000000000000010,
    TreeChop = 0b0000000000000100,
    Burnable = 0b0000000000001000,
    Light =    0b0000000000010000,
    Heal =     0b0000000000100000,
}

public enum WeaponType
{
    None, Melee, Gun
}

/// <summary>
///     Class for one item. An item's stats and other major values are stored within the ItemAttributes,
///     and modifiable data for an item is stored here (stack count, current durability, etc).
/// </summary>
public class Item
{
    private readonly ItemAttributes attributes;
    private int stackCount;
    private int currentDurability;
    private int ammoCount;

    public Item(ItemAttributes attributest)
    {
        attributes = attributest;
        stackCount = 1;
        currentDurability = attributest.WeaponDurability > 0 ? attributest.WeaponDurability : attributest.LightDurability;
        ammoCount = attributest.GunMagazineSize;
    }

    public ItemAttributes Stats { get { return attributes; } }

    public bool IsType(ItemType type)
    {
        return (attributes.Type & (int) type) == (int) type;
    }

    public int GetCount()
    {
        return stackCount;
    }

    public void DecrementCount()
    {
        stackCount--;
    }

    public void IncrementCount()
    {
        stackCount++;
    }

    public int GetDurability()
    {
        return currentDurability;
    }

    public void DecrementDurability()
    {
        currentDurability--;
        //Debug.Log(currentDurability);
    }

    public void SetDurability(int durability)
    {
        currentDurability = durability;
    }

    public int GetCurrentAmmo()
    {
        return ammoCount;
    }

    public void DecrementAmmo()
    {
        ammoCount--;
    }
}
