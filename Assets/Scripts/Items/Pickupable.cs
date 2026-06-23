using UnityEngine;

namespace Permafrost.Items
{
    /// <summary>
    /// Simple component to make an item that can be picked up in-world.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Pickupable : MonoBehaviour
    {
        #region Data
        [SerializeField] private ItemAttributes item;
        /// <summary>
        /// The attributes of the item this pickup is for.
        /// </summary>
        public ItemAttributes Item { get { return item; } }

        private Collider pickupTrigger;
        private float pickupLockout;
        public bool Collected { get; private set; } = false;
        #endregion

        #region Methods
        // Setup, uses first trigger collider as the pickup trigger and sets the lockout time
        private void Awake()
        {
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider.isTrigger)
                {
                    pickupTrigger = collider;
                    break;
                }
            }
            pickupTrigger.enabled = false;
            pickupLockout = 0.5f;
        }

        // just sets the collected value to true
        public void Collect()
        {
            Collected = true;
        }

        // runs the pickup lockout timer
        private void Update()
        {
            pickupLockout -= Time.deltaTime;
            if (pickupLockout < 0)
            {
                pickupTrigger.enabled = true;
            }
        }
        #endregion
    }
}
// 20 SLOC