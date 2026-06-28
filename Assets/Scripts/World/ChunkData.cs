using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// A component intended to be added to a terrain object to contain 
    /// specifc data, like the biome types.
    /// </summary>
    public class ChunkData : MonoBehaviour
    {
        public Vector3 CellLocation { get; private set; }
        public float ForestationFactor { get; private set; }
        public float DryFactor { get; private set; }

        private bool valuesSet;
        public void SetValues(Vector3 location, float forestation, float dry)
        {
            if (valuesSet) return;
            CellLocation = location;
            ForestationFactor = forestation;
            DryFactor = dry;
            valuesSet = true;
        }
    }

}