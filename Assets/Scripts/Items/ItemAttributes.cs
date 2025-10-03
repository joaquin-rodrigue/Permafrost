using UnityEngine;

[CreateAssetMenu]
public class ItemAttributes : ScriptableObject
{
    public int Type;
    public string Name;
    public int MaxStackSize;
    public int WeaponDurability;
    public int WeaponDamage;
    public int FoodHungerRestore;
    public int BurnableFuelValue;
}
