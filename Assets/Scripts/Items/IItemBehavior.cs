using Permafrost.Player;
using UnityEngine;

namespace Permafrost.Items
{
    /// <summary>
    /// The interface for all item behaviors.
    /// Ideally, classes implementing this interface should also
    /// implement a singleton structure.
    /// </summary>
    public interface IItemBehavior
    {
        /// <summary>
        /// Any and all code meant to be ran every frame.
        /// </summary>
        /// <param name="inventory">The player's inventory.</param>
        /// <param name="self">The item object this update is for.</param>
        public void Update(PlayerInventory inventory, Item self);

        /// <summary>
        /// Any code to be ran when the item is used.
        /// Note: durability drop is not done by the inventory, as I believe items
        /// should be able to drop durability by more than one point at a time.
        /// </summary>
        /// <param name="inventory">The player's inventory.</param>
        /// <param name="self">The item object this update is for.</param>
        /// <returns>True if the item count should be decremented, false otherwise.</returns>
        public bool Use(PlayerInventory inventory, Item self);
    }
}
