using UnityEngine;

/// <summary>
///     Set of stats/attributes for an Item object.
/// </summary>
[CreateAssetMenu]
public class ItemAttributes : ScriptableObject
{
    public int Type;
    public string Name;
    public int MaxStackSize;
    public int WeaponDurability;
    public int WeaponDamage;
    public float WeaponHungerLoss;
    public int FoodHungerRestore;
    public int BurnableFuelValue;
    public int BreakingStrength;
    public int LightDurability;
    public int LightIntensity;
    public int LightRange;
    public Color LightColor;
}
