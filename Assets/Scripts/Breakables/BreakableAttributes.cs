using UnityEngine;

namespace Permafrost.Items
{
    /// <summary>
    ///     Set of stats/attributes for a Breakable game object.
    /// </summary>
    [CreateAssetMenu]
    public class BreakableAttributes : ScriptableObject
    {
        public int Durability;
        public GameObject DropItem;
        public int MinDropCount;
        public int MaxDropCount;
        public ItemType TypeForBreaking;
        public SoundType OnHitSoundType;
    }

}