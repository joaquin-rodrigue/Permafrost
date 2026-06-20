using Permafrost.Player;
using UnityEngine;

namespace Permafrost.Items
{
    /// <summary>
    /// Enumerator for the various item types. Item types themselves are stored as an integer,
    /// and this enum corresponds the various bits of an int to different item types. If you're
    /// trying to determine if an item is of a certain type, you should use the <c>IsType()</c>
    /// method on the Item object for simplicity.
    /// </summary>
    public enum ItemType
    {
        Food =      0b0000000000000001,
        Weapon =    0b0000000000000010,
        TreeChop =  0b0000000000000100,
        Burnable =  0b0000000000001000,
        Light =     0b0000000000010000,
        Heal =      0b0000000000100000,
        WallBreak = 0b0000000001000000,
        Wearable =  0b0000000010000000,
    }

    /// <summary>
    /// A simple enum for what type of weapon the item is. None implies it is not a weapon,
    /// Melee is weapons with a swinging animation, and gun is ranged hitscan that uses ammo.
    /// </summary>
    public enum WeaponType
    {
        None, Melee, Thrown, MeleeAndThrown, Gun
    }

    /// <summary>
    /// A simple enum for what type of wearable item an item is. None implies it's not wearable,
    /// Body, Head and Foot should be self explanatory.
    /// </summary>
    public enum WearableType
    {
        None, Body, Head, Foot
    }

    /// <summary>
    /// A simple enum for what type of sounds an item makes.
    /// TODO: deprecated? nto sure right now
    /// </summary>
    public enum SoundType
    {
        WoodHit, FastSwing, SlowSwing
    }

    /// <summary>
    /// Class for one item. An item's stats and other major values are stored within the ItemAttributes,
    /// the item's behavior functions are attached in the IItemBehavior classes,
    /// and modifiable data for an item is stored here (stack count, current durability, etc).
    /// </summary>
    public class Item
    {
        #region Data
        private readonly ItemAttributes attributes;
        private readonly IItemBehavior behavior;
        private int stackCount;
        private int currentDurability;
        private int ammoCount;

        /// <summary>
        /// Provides the immutable stats of a given item.
        /// </summary>
        public ItemAttributes Stats { get { return attributes; } }

        /// <summary>
        /// Not sure why you'd want this, but if you really need to check the behavior type of an item without using its name/id go for it bud.
        /// </summary>
        public IItemBehavior Behavior { get { return behavior; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an Item based on the given attributes and behaviors.
        /// </summary>
        /// <param name="attributest">The ItemAttributes this item is based on.</param>
        /// <param name="behaviors">The ItemBehaviors this item will use.</param>
        public Item(ItemAttributes attributest, IItemBehavior behaviors)
        {
            attributes = attributest;
            behavior = behaviors;
            stackCount = 1;
            currentDurability = attributest.WeaponDurability > 0 ? attributest.WeaponDurability : attributest.LightDurability;
            ammoCount = attributest.GunMagazineSize;
        }

        public Item(ItemAttributes attributest)
        {
            attributes = attributest;
            stackCount = 1;
            currentDurability = attributest.WeaponDurability > 0 ? attributest.WeaponDurability : attributest.LightDurability;
            ammoCount = attributest.GunMagazineSize;

            // try to find the behavior class
            behavior = ItemLibrary.Instance.GetItemBehaviors(attributes.Name);
            if (behavior == null)
            {
                Debug.LogWarning($"Item '{attributes.Name}' made but no behavior class could be found!");
            }
        }

        public Item(Item original)
        {
            attributes = original.Stats;
            behavior = original.Behavior;
            stackCount = original.GetCount();
            currentDurability = original.GetDurability();
            ammoCount = original.GetCurrentAmmo();
        }
        #endregion

        #region Behavior Hooks
        /// <summary>
        /// Calls the attached behavior's Update method.
        /// Generally, this will be called every frame.
        /// </summary>
        /// <param name="caller">The inventory calling the update.</param>
        public void Update(PlayerInventory caller)
        {
            Behavior.Update(caller, this);
        }

        /// <summary>
        /// Calls the attached behavior's Use method.
        /// Returns true when the item stack should be decremented.
        /// </summary>
        /// <param name="caller">The inventory calling the update.</param>
        /// <returns>True if the item stack should be decremented, false otherwise.</returns>
        public bool Use(PlayerInventory caller)
        {
            return Behavior.Use(caller, this);
        }
        #endregion

        #region All the Other Things I guess idk what to name this region
        /// <summary>
        /// Determines if the given item includes the given item type.
        /// </summary>
        /// <param name="type">The ItemType to check for.</param>
        /// <returns>True if the item includes the given item type, false otherwise.</returns>
        public bool IsType(ItemType type)
        {
            return (attributes.Type & (int)type) == (int)type;
        }

        /// <summary>
        /// Determines if the given item is the same as an existing one.
        /// </summary>
        /// <param name="item">The Item to check if it is the same.</param>
        /// <returns>True if the items are the same kind, false otherwise.</returns>
        public bool SameStackAs(Item item)
        {
            if (item == null) return false;
            return item.Stats.Name == Stats.Name;
        }

        /// <summary>
        /// Returns the stack count.
        /// </summary>
        /// <returns>The number of items in this item's stack.</returns>
        public int GetCount()
        {
            return stackCount;
        }

        /// <summary>
        /// Decrements the stack count.
        /// </summary>
        public void DecrementCount()
        {
            stackCount--;
        }

        /// <summary>
        /// Increments the stack count.
        /// </summary>
        public void IncrementCount()
        {
            stackCount++;
        }

        /// <summary>
        /// Gets the item's current durability.
        /// </summary>
        /// <returns>The item's current durability.</returns>
        public int GetDurability()
        {
            return currentDurability;
        }

        /// <summary>
        /// Decrements the item's current durability.
        /// </summary>
        public void DecrementDurability()
        {
            currentDurability--;
        }

        // damn. this just didn't do bounds checking until now.
        /// <summary>
        /// Sets the item's current durability to a new value.
        /// </summary>
        /// <param name="durability">The new durability for the item.</param>
        public void SetDurability(int durability)
        {
            if (durability < 0) currentDurability = 0;
            if (durability > Stats.WeaponDurability && durability > Stats.LightDurability) currentDurability = Mathf.Max(Stats.WeaponDurability, Stats.LightDurability);
            currentDurability = durability;
        }

        /// <summary>
        /// Gets the current ammo count.
        /// </summary>
        /// <returns>The item's current ammo count.</returns>
        public int GetCurrentAmmo()
        {
            return ammoCount;
        }

        /// <summary>
        /// Decreases the item's ammo count.
        /// </summary>
        public void DecrementAmmo()
        {
            ammoCount--;
        }

        /// <summary>
        /// Sets the item's ammo count.
        /// </summary>
        /// <param name="ammo">The new ammo count for the item.</param>
        public void SetAmmo(int ammo)
        {
            if (ammo < 0) ammoCount = 0;
            if (ammo > Stats.GunMagazineSize) ammoCount = Stats.GunMagazineSize;
            ammoCount = ammo;
        }
        #endregion
    }
}
