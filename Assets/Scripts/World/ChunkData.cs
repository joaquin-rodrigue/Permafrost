using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// A component intended to be added to a terrain object to contain 
    /// specifc data, like the biome types.
    /// </summary>
    public class ChunkData : MonoBehaviour
    {
        public float ForestationFactor { get; private set; }
        public float DryFactor { get; private set; }

        private bool valuesSet;
        public void SetValues(float forestation, float dry)
        {
            if (valuesSet) return;
            ForestationFactor = forestation;
            DryFactor = dry;
            valuesSet = true;
        }
    }

}