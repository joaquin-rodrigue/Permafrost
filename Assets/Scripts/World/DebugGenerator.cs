#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Permafrost.World
{
    public class DebugGenerator : MonoBehaviour
    {
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
                    gen.GenerateNewBlock(new Vector3(i * gen.TerrainCellSize, 0, j * gen.TerrainCellSize));
                }
            }
        }

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
                    gen.GenerateNewBlock(new Vector3(i * gen.TerrainCellSize, 0, j * gen.TerrainCellSize));
                }
            }
        }
    }
}
#endif