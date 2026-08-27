using Permafrost.Player;
using UnityEngine;

namespace Permafrost.Items
{
    public class BasicMeleeWeaponBehavior : IItemBehavior
    {
        public BasicMeleeWeaponBehavior() { }

        public void Update(PlayerInventory inventory, Item self)
        {
            
        }

        public bool Use(PlayerInventory inventory, Item self)
        {
            return self.GetDurability() > 0;
        }
    }

}