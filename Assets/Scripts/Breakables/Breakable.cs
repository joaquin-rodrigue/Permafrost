using UnityEngine;

namespace Permafrost.Items
{
    /// <summary>
    /// A waay to designate the given game object as breakable. Static data is stored
    /// in BreakableAttributes, and modifiable data (mainly remaining durability) is 
    /// stored here.
    /// </summary>
    /// <remarks>
    /// This class doesn't contain any real functionality, breaking an object is handled by the player classes.
    /// </remarks>
    [RequireComponent(typeof(Collider))]
    public class Breakable : MonoBehaviour
    {
        [SerializeField] private BreakableAttributes stats;
        private int currentDurability;

        public BreakableAttributes Stats { get { return stats; } }

        private void Awake()
        {
            currentDurability = stats.Durability;
        }

        public void DecreaseDurability(int damage)
        {
            currentDurability -= damage;
        }

        public int GetDurability()
        {
            return currentDurability;
        }
    }
}