using UnityEngine;

namespace Permafrost.Items
{
    /// <summary>
    /// Set of stats/attributes for an Item object.
    /// </summary>
    [CreateAssetMenu]
    public class ItemAttributes : ScriptableObject
    {
        [Header("General")]
        public int Type;
        [Tooltip("Treated as an ID string; if two items have the same ID expect bugs!")]
        public string Name;
        public int MaxStackSize;

        [Header("Physical")]
        public GameObject Prefab;
        public Mesh ViewModel;
        public Material ViewModelMaterial;
        public Vector3 ViewModelScale;
        public Quaternion ViewModelRotation;
        public Sprite InventoryRender;

        [Header("Weapon Specific")]
        public WeaponType WeaponAttack;
        public int WeaponDurability;
        public int WeaponDamage;
        public float WeaponHungerLoss;
        public ItemAttributes GunBulletType;
        public int GunMagazineSize;

        [Header("Consumables")]
        public int FoodHungerRestore;
        public int HealthRestore;
        public float HealthRegenerationTime;
        public float HealthRegenerationSpeed;
        public int BurnableFuelValue;
        public int BurnableFuelStrength;

        [Header("Tools")]
        public int BreakingStrength;
        public WearableType WearableItemType;
        public float WearableDamageModifier;
        public float WearableTemperatureLossModifier;
        public float WearableSpeedModifier;

        [Header("Lights")]
        public int LightDurability;
        public int LightIntensity;
        public int LightRange;
        public Color LightColor;
    }
}
// 31 SLOC