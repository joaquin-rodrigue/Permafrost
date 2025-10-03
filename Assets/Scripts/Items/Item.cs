public enum ItemType
{
    Food =     0b00000001,
    Weapon =   0b00000010,
    TreeChop = 0b00000100,
    Burnable = 0b00001000,
}

public class Item
{
    private readonly ItemAttributes attributes;
    private int stackCount;
    private int currentDurability;

    public Item(ItemAttributes attributest)
    {
        attributes = attributest;
        stackCount = 1;
        currentDurability = attributest.WeaponDurability;
    }

    public ItemAttributes GetAttributes()
    {
        return attributes;
    }

    public int GetCount()
    {
        return stackCount;
    }

    public void DecreaseCount()
    {
        stackCount--;
    }

    public void IncreaseCount()
    {
        stackCount++;
    }

    public int GetDurability()
    {
        return currentDurability;
    }

    public void SetDurability(int durability)
    {
        currentDurability = durability;
    }
}
