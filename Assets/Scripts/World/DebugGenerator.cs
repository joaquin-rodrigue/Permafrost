#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// Used for debug menu options to make world generation testing.
    /// </summary>
    public class DebugGenerator : MonoBehaviour
    {
        /// <summary>
        /// Generates a single terrain cell using the master terrain generator's current settings.
        /// </summary>
        [MenuItem("Generation/Generate One Block")]
        public static void GenerateOneBlock()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            MasterTerrainGenerator gen = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<MasterTerrainGenerator>();
            gen.Activate();
            gen.ClearAllBlocks();
            gen.GenerateNewBlock(Vector3.zero);
        }

        /// <summary>
        /// Generates a 3x3 square of terrain cells using the master terrain generator's current settings.
        /// </summary>
        [MenuItem("Generation/Generate 3 by 3")]
        public static void GenerateThreeByThree()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            MasterTerrainGenerator gen = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<MasterTerrainGenerator>();
            gen.Activate();
            gen.ClearAllBlocks();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    gen.QueueTerrain(new Vector3(i * gen.TerrainCellSize, 0, j * gen.TerrainCellSize));
                }
            }
        }

        /// <summary>
        /// Generates a 11x11 grid of terrain cells, or 121, using the master terrain generator's current settings.
        /// </summary>
        [MenuItem("Generation/Generate Many Blocks")]
        public static void GenerateOneHundo()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            MasterTerrainGenerator gen = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<MasterTerrainGenerator>();
            gen.Activate();
            gen.ClearAllBlocks();
            for (int i = -5; i < 6; i++)
            {
                for (int j = -5; j < 6; j++)
                {
                    gen.QueueTerrain(new Vector3(i * gen.TerrainCellSize, 0, j * gen.TerrainCellSize));
                }
            }
        }

        /// <summary>
        /// Generates a 41x41 grid of terrain cells using the master terrain generator's current settings.
        /// This is actually *bigger* than 400, this is 1681 terrain cells. Generates significant lag, may
        /// also work as a good lag test.
        /// </summary>
        [MenuItem("Generation/Generate A TON of blocks")]
        public static void GenerateFourHundo()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Must be used in play mode!");
                return;
            }

            MasterTerrainGenerator gen = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<MasterTerrainGenerator>();
            gen.Activate();
            gen.ClearAllBlocks();
            for (int i = -20; i < 21; i++)
            {
                for (int j = -20; j < 21; j++)
                {
                    gen.QueueTerrain(new Vector3(i * gen.TerrainCellSize, 0, j * gen.TerrainCellSize));
                }
            }
        }
    }
}
#endif
// SLOC not counted