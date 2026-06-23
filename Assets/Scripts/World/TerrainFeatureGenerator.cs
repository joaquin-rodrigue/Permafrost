using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(MasterTerrainGenerator))]
    public class TerrainFeatureGenerator : MonoBehaviour
    {
        #region Data
        [Header("In-World Objects")]
        [SerializeField] private GameObject[] treeTypes;
        [SerializeField] private GameObject[] bushTypes;
        [SerializeField] private GameObject[] fences;

        [Header("Ruined Shack Objects")]
        [SerializeField] private float TODO;

        [Header("Ruined Barn Objects")]
        [SerializeField] private float TODDO;

        [Header("Tree Config")]
        [SerializeField] private int maxTreesPerBlock;
        [SerializeField] private int minTreesPerBlock;
        [SerializeField] private int maxTreesPerCluster;
        [SerializeField] private int minTreesPerCluster;
        [SerializeField] private float chanceToOverrideTreeCounts;

        [Header("Bush Config")]
        [SerializeField] private int maxBushesPerBlock;
        [SerializeField] private int minBushesPerBlock;
        [SerializeField] private int maxBushesPerCluster;
        [SerializeField] private int minBushesPerCluster;
        [SerializeField] private float chanceToOverrideBushCounts;

        [Header("Fence Config")]
        [SerializeField] private int maxFencesPerBlock;
        [SerializeField] private int minFencesPerBlock;
        [SerializeField] private float chanceToOverrideFenceCounts;

        [Header("Other Settings")]
        [Tooltip("Every looping operation, if more than this many loops happen, then the generation step ends and the next step begins.")]
        [SerializeField] private int maxLoopCountDuringGenerationSteps = 1000;
        [SerializeField] private LayerMask terrainLayerMask;
        [SerializeField] private float heightRaycastDistance;

        [Header("Component References")]

        private MasterTerrainGenerator masterGenerator;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Unity Methods
        // setup
        private void Awake()
        {
            masterGenerator = GetComponent<MasterTerrainGenerator>();
        }
        #endregion

        #region Generation Phases
        /// <summary>
        /// Generates all the objects for a given terrain object.
        /// </summary>
        /// <param name="terrain">The GameObject for the current terrain cell.</param>
        public void GenerateObjectsFor(GameObject terrain)
        {
            Bounds cellBounds = new(
                terrain.transform.position + new Vector3(masterGenerator.TerrainCellSize / 2, 0, masterGenerator.TerrainCellSize / 2),
                new Vector3(masterGenerator.TerrainCellSize, 1, masterGenerator.TerrainCellSize));
            System.Random RNG = masterGenerator.ObjectRNG;

            GenerateTreesAndBushes(terrain, cellBounds, RNG);
        }

        /// <summary>
        /// Phase One: Generates the trees and bushes around the map.
        /// </summary>
        /// <param name="terrain">The GameObject for the current terrain cell.</param>
        /// <param name="cellBounds">The bounds of the current terrain cell (although only x and z components are currently used).</param>
        /// <param name="RNG">The Master Terrain Generator's Object RNG.</param>
        private void GenerateTreesAndBushes(GameObject terrain, Bounds cellBounds, System.Random RNG)
        {
            // first we decide counts
            bool overridenLimits = RNG.NextDouble() < chanceToOverrideTreeCounts;
            int lowerBound = minTreesPerBlock;
            int upperBound = maxTreesPerBlock;
            while (overridenLimits)
            {
                lowerBound /= 2;
                upperBound *= 2;
                overridenLimits = RNG.NextDouble() < chanceToOverrideTreeCounts;
            }

            // now we make tree
            int treeCount = RNG.Next(lowerBound, upperBound);
            bool foundValidSpawn = false;
            RaycastHit hit = new();
            int currentLoopCount = 0;
            if (debugEnabled) Debug.Log($"[TerrainFeatureGenerator] Cell tree count goal: {treeCount}");

            for (int i = 0; i < treeCount && currentLoopCount < maxLoopCountDuringGenerationSteps;)
            {
                foundValidSpawn = false;
                while (!foundValidSpawn && currentLoopCount < maxLoopCountDuringGenerationSteps)
                {
                    foundValidSpawn = Physics.Raycast(new Vector3(
                        RNG.Next((int) cellBounds.min.x, (int) cellBounds.max.x),
                        heightRaycastDistance,
                        RNG.Next((int) cellBounds.min.z, (int) cellBounds.max.z)),
                        Vector3.down,
                        out hit,
                        heightRaycastDistance,
                        terrainLayerMask);
                    currentLoopCount++;
                }

                // potential abort here if loops are over the limit
                if (currentLoopCount >= maxLoopCountDuringGenerationSteps)
                {
                    if (debugEnabled) Debug.Log($"[TerrainFeatureGenerator] Trees ran past the max loop count, aborting tree step!");
                    break;
                }

                int countInArea = RNG.Next(minTreesPerCluster, maxTreesPerCluster);
                int treeTypeIndex = RNG.Next(0, treeTypes.Length - 1);
                for (int j = 0; j < countInArea; j++)
                {
                    RaycastHit chosenPoint = new();
                    // you know what? i'll still admit, this is wacky
                    while (!Physics.Raycast(new Vector3(
                        hit.point.x + RNG.Next(-10, 10),
                        hit.point.y + (heightRaycastDistance / 4),
                        hit.point.z + RNG.Next(-10, 10)),
                        Vector3.down,
                        out chosenPoint,
                        heightRaycastDistance / 2,
                        terrainLayerMask
                    ) && currentLoopCount < maxLoopCountDuringGenerationSteps) currentLoopCount++;

                    GameObject currentObj = Instantiate(treeTypes[treeTypeIndex], terrain.transform);
                    currentObj.transform.SetPositionAndRotation(chosenPoint.point, Quaternion.Euler(0, RNG.Next(0, 360), 0));
                    currentObj.transform.localScale = new Vector3((float) RNG.NextDouble() * 0.5f + 0.95f, (float) RNG.NextDouble() * 0.5f + 0.95f, (float) RNG.NextDouble() * 0.5f + 0.95f);
                    i++;

                    if (debugEnabled)
                    {
                        Debug.Log($"[TerrainFeatureGenerator] Tree built at {currentObj.transform.position}");
                    }
                }
            }

            // now for bush
            overridenLimits = RNG.NextDouble() < chanceToOverrideBushCounts;
            lowerBound = minBushesPerBlock;
            upperBound = maxBushesPerBlock;
            while (overridenLimits)
            {
                lowerBound /= 2;
                upperBound *= 2;
                overridenLimits = RNG.NextDouble() < chanceToOverrideTreeCounts;
            }

            // george bush
            int bushCount = RNG.Next(lowerBound, upperBound);
            currentLoopCount = 0;
            if (debugEnabled) Debug.Log($"[TerrainFeatureGenerator] Cell bush count goal: {bushCount}");

            for (int i = 0; i < bushCount && currentLoopCount < maxLoopCountDuringGenerationSteps;)
            {
                foundValidSpawn = false;
                while (!foundValidSpawn && currentLoopCount < maxLoopCountDuringGenerationSteps)
                {
                    foundValidSpawn = Physics.Raycast(new Vector3(
                        RNG.Next((int)cellBounds.min.x, (int)cellBounds.max.x),
                        heightRaycastDistance,
                        RNG.Next((int)cellBounds.min.z, (int)cellBounds.max.z)),
                        Vector3.down,
                        out hit,
                        heightRaycastDistance,
                        terrainLayerMask);
                    currentLoopCount++;
                }

                // george w. bush
                if (currentLoopCount >= maxLoopCountDuringGenerationSteps)
                {
                    if (debugEnabled) Debug.Log($"[TerrainFeatureGenerator] Bushes ran past the max loop count, aborting bush step!");
                    break;
                }

                int countInArea = RNG.Next(minBushesPerCluster, maxBushesPerCluster);
                int bushTypeIndex = RNG.Next(0, bushTypes.Length);
                for (int j = 0; j < countInArea; j++)
                {
                    RaycastHit chosenPoint = new();
                    // george h. w. bush
                    while (!Physics.Raycast(new Vector3(
                        hit.point.x + RNG.Next(-10, 10),
                        hit.point.y + (heightRaycastDistance / 4),
                        hit.point.z + RNG.Next(-10, 10)),
                        Vector3.down,
                        out chosenPoint,
                        heightRaycastDistance / 2,
                        terrainLayerMask
                    ) && currentLoopCount < maxLoopCountDuringGenerationSteps) currentLoopCount++;

                    GameObject currentObj = Instantiate(bushTypes[bushTypeIndex], terrain.transform);
                    currentObj.transform.SetPositionAndRotation(chosenPoint.point, Quaternion.Euler(0, RNG.Next(0, 360), 0));
                    currentObj.transform.localScale = new Vector3((float) RNG.NextDouble() * 0.5f + 0.95f, (float) RNG.NextDouble() * 0.5f + 0.95f, (float) RNG.NextDouble() * 0.5f + 0.95f);
                    i++;

                    if (debugEnabled)
                    {
                        Debug.Log($"[TerrainFeatureGenerator] Bush generated at {currentObj.transform.position}");
                    }
                }
            }
        }
        #endregion
    }
}