using System.Collections;
using UnityEngine;

namespace Permafrost.Player
{
    /// <summary>
    /// A container for the player's inventory. Can change size (if needed) and
    /// handle functions for adding, removing, etc. items in the inventory. Also 
    /// runs any update/usage code for items, and handles dropping items.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Data
        [Header("General")]
        [SerializeField] private int inventorySize = 9;
        [SerializeField] private float useItemCooldown = 0.25f;
        [SerializeField] private float dropItemCooldown = 0.25f;
        [Tooltip("Every item dropped multiplies the next item drop cooldown by this amount. Effectively, holding drop will increase the speed of dropping further items based on this multiplier.")]
        [SerializeField] private float dropItemCooldownModifier = 0.95f;

        // Internal state
        private Item[] inventory;
        private float currentDropItemTime;
        private int currentDropItemStreak;

        // All the script getter/setter stuff
        public int SelectedItemIndex { get; private set; }
        public Item SelectedItem { get => inventory[SelectedItemIndex]; }
        public int AvailableInventorySlots { get; private set; }
        public bool InventoryFull { get => AvailableInventorySlots == inventorySize; }
        public bool CanUseItem { get; private set; }
        public bool CanDropItem { get; private set; }
        public bool ItemSwitchingActive { get; set; }

        [Header("Component References")]
        //[SerializeField] private GameMaster gameMaster;
        [SerializeField] private PlayerController playerController;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Unity Methods
        // Setup (very simple for once)
        private void Awake()
        {
            EmptyInventory();
        }

        // Update
        private void FixedUpdate()
        {
            //if (gameMaster.GamePaused) return;
            ItemUpdate();
            UseItem();
            DropItem();
        }
        #endregion

        #region Inventory Update
        /// <summary>
        /// Updates every item in the inventory. If an item doesn't have a defined
        /// update function, it effectively does nothing here.
        /// </summary>
        private void ItemUpdate()
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null) continue;

                ItemAttributes stats = inventory[i].Stats;

                Debug.LogWarning("todo: item update function in item class");
                //SelectedItem.Update();
            }
        }
        
        /// <summary>
        /// Checks if the player is trying to and can use an item, and if
        /// so, uses the player's selected item.
        /// </summary>
        private void UseItem()
        {
            if (SelectedItem == null) return;
            if (!playerController.UsingItem || !CanUseItem) return;

            StartCoroutine(UseItemCooldown());
            bool shouldDecreaseItemCount = true; //SelectedItem.Use();
            if (shouldDecreaseItemCount) DecrementSelectedItemCount();
        }

        /// <summary>
        /// Runs a cooldown between item uses.
        /// </summary>
        /// <returns>After useItemCooldown seconds.</returns>
        private IEnumerator UseItemCooldown()
        {
            CanUseItem = false;
            yield return new WaitForSeconds(useItemCooldown);
            CanUseItem = true;
        }

        /// <summary>
        /// Decreases the count of the currently selected item.
        /// If the item's stack has a count of 0 after this, the item is deleted.
        /// </summary>
        private void DecrementSelectedItemCount()
        {
            SelectedItem.DecrementCount();
            if (SelectedItem.GetCount() <= 0)
            {
                inventory[SelectedItemIndex] = null;
            }
        }

        /// <summary>
        /// Checks if the player can drop the current item, and if they are trying
        /// to drop it. If so, the currently selected item is dropped, and instantiated
        /// in world.
        /// </summary>
        private void DropItem()
        {
            if (SelectedItem == null) return;
            if (!CanDropItem) return;
            if (!playerController.DropItem)
            {
                currentDropItemStreak = 0;
                currentDropItemTime = dropItemCooldown;
                return;
            }

            StartCoroutine(DropItemCooldown());
            //itemLibrary.CreatePhysicalItem(SelectedItem, transform);
            DecrementSelectedItemCount();
        }

        /// <summary>
        /// Runs the cooldown for dropping items. If the player holds the drop button,
        /// this also decreases the drop timer during consecutive drops to speed up
        /// the process.
        /// </summary>
        /// <returns>After currentDropItemTime seconds, which decreases the more drops are done consecutively in a row.</returns>
        private IEnumerator DropItemCooldown()
        {
            currentDropItemStreak++;
            currentDropItemTime *= dropItemCooldownModifier;
            CanDropItem = false;
            yield return new WaitForSeconds(currentDropItemTime);
            CanDropItem = true;
        }
        #endregion

        #region Inventory Stuff
        /// <summary>
        /// Sets the player's inventory to a blank array.
        /// </summary>
        public void EmptyInventory()
        {
            inventory = new Item[inventorySize];
            AvailableInventorySlots = inventorySize;
        }

        /// <summary>
        /// Changes the inventory's current size. If the inventory becomes smaller,
        /// this may attempt to re-arrange items to fit into the smaller space.
        /// </summary>
        /// <param name="newSize">The new size for the inventory.</param>
        /// <returns>True if the inventory could be modified successfully, false otherwise. If true is returned, the inventory may have changed.</returns>
        public bool ModifyInventorySize(int newSize)
        {
            if (newSize < 0) return false;

            if (newSize < inventorySize) return CompressInventory(newSize);
            else return ExpandInventory(newSize);
        }

        /// <summary>
        /// Attempts to compress the inventory to the new size. If successful, the inventory
        /// is updated to the new, compressed version, and if not, the process is aborted.
        /// </summary>
        /// <param name="size">The new size for the inventory.</param>
        /// <returns>True if the inventory was compressed successfully, false otherwise.</returns>
        private bool CompressInventory(int size)
        {
            if (inventorySize - AvailableInventorySlots > size) return false;

            int openIndex = 0;
            int closedIndex = size;

            // make a new temp array to try to compress to
            Item[] temp = new Item[size];
            for (int i = 0; i < size; i++)
            {
                temp[i] = inventory[i];
            }

            // try and compress
            for (; closedIndex < inventorySize; closedIndex++)
            {
                if (inventory[closedIndex] == null) continue;
                for (; openIndex < size; openIndex++)
                {
                    if (temp[openIndex] == null)
                    {
                        temp[openIndex] = inventory[closedIndex];
                        break;
                    }
                }
                if (openIndex == size) return false;
            }

            // success!!!! yippee!!!!
            inventory = temp;
            AvailableInventorySlots -= inventorySize - size;
            if (SelectedItemIndex > size) SelectedItemIndex = 0;
            inventorySize = size;

            return true;
        }

        /// <summary>
        /// Expands the inventory to the new size.
        /// </summary>
        /// <param name="size">The new size of the inventory.</param>
        /// <returns>True, always. Mostly to fulfill the usage in <c>ModifyInventorySize()</c>.</returns>
        private bool ExpandInventory(int size)
        {
            Item[] temp = new Item[size];
            for (int i = 0; i < inventorySize; i++) temp[i] = inventory[i];
            inventorySize = size;
            inventory = temp;
            return true;
        }

        /// <summary>
        /// Adds an item to the inventory. If a stack of that item is already in the
        /// inventory, the stack is incremented. Otherwise, if an open space is available,
        /// that space will be turned into the new item stack. If no spaces are open,
        /// the item is not added.
        /// </summary>
        /// <param name="item">The Item to add to the inventory.</param>
        /// <returns>True if the item was added to either an existing or new stack, false otherwise.</returns>
        public bool AddItem(Item item)
        {
            // try add item to existing stack
            for (int i = 0; i < inventorySize; i++)
            {
                //if (!inventory[i].SameStackAs(item)) continue;
                if (inventory[i].GetCount() >= inventory[i].Stats.MaxStackSize) continue;

                inventory[i].IncrementCount();
                Debug.LogWarning("todo: item pickup stuff?");
                return true;
            }

            // try add item to new stack
            if (AvailableInventorySlots == 0) return false;
            for (int i = 0; i < inventorySize; i++)
            {
                if (inventory[i] != null) continue;

                //inventory[i] = new Item(item);
                Debug.LogWarning("todo: item pickup stuff?");
                AvailableInventorySlots--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Swaps the position of two items in the inventory.
        /// </summary>
        /// <param name="one">The index of the first item to swap.</param>
        /// <param name="two">The index of the second item to swap.</param>
        public void SwapItems(int one, int two)
        {
            (inventory[two], inventory[one]) = (inventory[one], inventory[two]);
        }

        /// <summary>
        /// Removes an item from the inventory. This item is deleted permanently, be warned.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        /// <returns>True if an item was removed successfully, false otherwise.</returns>
        public bool RemoveItem(int index)
        {
            if (inventory[index] == null) return false;

            inventory[index] = null;
            Debug.LogWarning("todo: item removal stuff?");
            AvailableInventorySlots++;
            return true;
        }
        #endregion
    }
}
