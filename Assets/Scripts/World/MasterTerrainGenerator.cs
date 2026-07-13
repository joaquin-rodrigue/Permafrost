using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Permafrost.World
{
    /// <summary>
    /// TODO: store to files?
    /// TODO: load from files?
    /// The terrain generator's main component. Coordinates all terrain cell generation,
    /// cell loading, cell unloading, etc.
    /// </summary>
    [RequireComponent(typeof(SimplexGenerator))]
    public class MasterTerrainGenerator : MonoBehaviour
    {
        #region Data
        [Header("Terrain Object Settings")]
        [SerializeField] private TerrainLayer[] terrainTextures;
        [SerializeField] private Material terrainMaterial;
        [Tooltip("Each cell is a square. Make sure this is some multiple of 2, else it will be set to the next largest multiple of 2.")]
        [SerializeField] private int terrainCellSize;
        [SerializeField] private int terrainDetailResolution = 64;
        [SerializeField] private int terrainDetailResolutionPerPatch = 16;

        public int TerrainCellSize { get => terrainCellSize; }
        private List<GameObject> activeTerrainObjects;
        private float[,] tempHeightsArray;

        [Header("Terrain Generation Settings")]
        [Tooltip("The center cell's cell position, please keep to integer components. Used mostly to figure out where to center the noise generation.")]
        [SerializeField] private Vector2 centerCellCoordinates;
        [SerializeField] private bool useRandomMasterSeed;
        [SerializeField] private int setMasterSeed;
        [SerializeField] private bool useRandomObjectSeed;
        [SerializeField] private int setObjectSeed;

        public Vector2 CenterCellLocation { get => centerCellCoordinates; }
        public int MasterSeed { get; private set; } = 0;
        public System.Random MasterRNG { get; private set; }
        public int ObjectSeed { get; private set; } = 0;
        public System.Random ObjectRNG { get; private set; }
        public bool Activated { get; private set; }

        private SimplexGenerator noiseGenerator;
        private FoliageGenerator featureGenerator;
        private PathGenerator pathGenerator;
        private bool featuresActive;
        private bool pathsActive;

        private List<Vector3> generationQueue;
        private bool generating;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        [SerializeField] private bool extensiveDebug;
        #endregion

        #region Unity Methods
        // Mostly just some starting checks/setup
        void Awake()
        {
            activeTerrainObjects = new();
            generationQueue = new();
            noiseGenerator = GetComponent<SimplexGenerator>();
            if (terrainMaterial == null)
            {
                Debug.LogWarning("[MasterTerrainGenerator] No terrain material assigned; terrains will not render properly!");
            }

            // instead of checking whether the terraincellsize is a power of 2
            // im just gonna do the loop regardless. Not worth the microoptimization
            int powerOfTwoCellSize = 2;
            while (powerOfTwoCellSize < terrainCellSize) powerOfTwoCellSize *= 2;

            if (debugEnabled)
            {
                Debug.Log($"[MasterTerrainGenerator] Cell Size (old): {terrainCellSize}, Cell Size (fixed to power of 2): {powerOfTwoCellSize}");
            }
            terrainCellSize = powerOfTwoCellSize;
            Activated = false;
        }

        // mostly to finish checking whether all the modules are active
        private void Start()
        {
            // and feature gen
            featuresActive = TryGetComponent(out featureGenerator);
            if (featuresActive) featuresActive = featureGenerator.isActiveAndEnabled;
            if (!featuresActive)
            {
                if (debugEnabled) Debug.Log("[MasterTerrainGenerator] No feature generator present/active, ignoring");
            }

            // and path gen
            pathsActive = TryGetComponent(out pathGenerator);
            if (pathsActive) pathsActive = pathGenerator.isActiveAndEnabled;
            if (!pathsActive)
            {
                if (debugEnabled) Debug.Log($"[MasterTerrainGenerator] No path generator present/active, ignoring");
            }
        }

        private void Update()
        {
            if (!generating && generationQueue.Count > 0)
            {
                Vector3 pos = generationQueue[0];
                generationQueue.RemoveAt(0);
                GenerateNewBlock(pos);
            }
        }
        #endregion

        #region The Basics
        /// <summary>
        /// This is the part that must be set up on a level-by-level basis.
        /// This includes resetting seed numbers/RNG objects, generating noise,
        /// and other setup that happens at the start of a seed.
        /// </summary>
        public void Activate()
        {
            MasterSeed = useRandomMasterSeed ? Random.Range(int.MinValue, int.MaxValue) : setMasterSeed;
            MasterRNG = new System.Random(MasterSeed);
            ObjectSeed = useRandomObjectSeed ? MasterRNG.Next(int.MinValue, int.MaxValue) : setObjectSeed;
            ObjectRNG = new System.Random(ObjectSeed);

            noiseGenerator.GeneratePermutations();
            Activated = true;

            // prints the simplex permutation tables into debug log
            if (extensiveDebug)
            {
                Debug.Log($"[MasterTerrainGenerator] From SimplexGenerator: Height Permutation Table:");
                byte[] heightmap = noiseGenerator.GetHeightmapPermutation();
                string list = "";
                foreach (byte b in heightmap)
                {
                    list += $"{b}, ";
                }
                Debug.Log($"[MasterTerrainGenerator] {list}");

                Debug.Log($"[MasterTerrainGenerator] From SimplexGenerator: Biome Permutation Table:");
                Unity.Mathematics.float3[] biomemap = noiseGenerator.GetBiomemapPermutation();
                list = "";
                foreach (Unity.Mathematics.float3 b in biomemap)
                {
                    list += $"{b}, ";
                }
                Debug.Log($"[MasterTerrainGenerator] {list}");
            }
        }

        /// <summary>
        /// Resets any number of the generator seeds.
        /// </summary>
        /// <param name="master">The master seed; if no other seeds are set, this will be used to determine the others and overall terrain shapes.</param>
        /// <param name="obj">The object seed; used to generate object details and structures.</param>
        public void ResetSeeds(int? master, int? obj)
        {
            useRandomMasterSeed = master.HasValue;
            useRandomObjectSeed = obj.HasValue;

            // todo: do we need to Activate() again or do we just need to reset the seed data? idk
            Activate();
        }
        #endregion

        #region Making Some Terra Ain
        public void QueueTerrain(Vector3 position)
        {
            generationQueue.Add(position);
            generationQueue.Sort((vec1, vec2) => (int)((Mathf.Abs(vec1.x) + Mathf.Abs(vec1.y) + Mathf.Abs(vec1.z)) - (Mathf.Abs(vec2.x) + Mathf.Abs(vec2.y) + Mathf.Abs(vec2.z))));
        }

        private void LoadBlockFromFile(Vector3 position)
        {

        }

        /// <summary>
        /// Clears all (active) terrain objects. Effectively deleting them.
        /// </summary>
        public void ClearAllBlocks()
        {
            for (int i = 0; i < activeTerrainObjects.Count; i++)
            {
                Destroy(activeTerrainObjects[i]);
            }
        }

        /// <summary>
        /// Generates a new terrain game object at the given position.
        /// </summary>
        /// <param name="position">The grid position to spawn the object at.</param>
        public void GenerateNewBlock(Vector3 position)
        {
            // checks
            if (generating)
            {
                QueueTerrain(position);
                return;
            }
            generating = true;

            if (debugEnabled)
            {
                Debug.Log($"[MasterTerrainGenerator] Generating cell at: {position}");
            }

            // terrain data comes first
            // this code, for some god awful reason, has to be ordered in this specific
            // way. if you try setting the terrain layers in any more concise a way, the
            // layers just aren't set under the hood. weirdest unity bug i've ever seen.
            // this is literally the "if i had a nickel for every time the world turned
            // pink, i'd have 2 nickels" situation.
            // I guess this works slightly better when you need more than one terrain layer, but still.
            TerrainData data = new();
            TerrainLayer[] layers = new TerrainLayer[terrainTextures.Length];
            for (int i = 0; i < layers.Length; i++) layers[i] = terrainTextures[i];
            data.terrainLayers = layers;

            // now we can set the rest of the data
            data.heightmapResolution = terrainCellSize + 1;
            data.size = new Vector3(terrainCellSize, 500, terrainCellSize);
            data.SetDetailResolution(terrainDetailResolution, terrainDetailResolutionPerPatch);
            data.alphamapResolution = terrainCellSize + 1;

            // texturize
            float[,,] alphamaps = data.GetAlphamaps(0, 0, terrainCellSize + 1, terrainCellSize + 1);
            int k = alphamaps.GetLength(2);
            for (int i = 0; i < alphamaps.GetLength(0); i++)
            {
                for (int j = 0; j < alphamaps.GetLength(1); j++)
                {
                    alphamaps[i, j, 0] = 0.5f;
                    alphamaps[i, j, k - 1] = 0.5f;
                }
            }
            data.SetAlphamaps(0, 0, alphamaps);

            Terrain newBlock = Terrain.CreateTerrainGameObject(data).GetComponent<Terrain>();
            activeTerrainObjects.Add(newBlock.gameObject);
            newBlock.transform.position = new Vector3(position.x, 0, position.z);
            newBlock.materialTemplate = terrainMaterial;
            newBlock.gameObject.layer = LayerMask.NameToLayer("Terrain");
            newBlock.GetComponent<TerrainCollider>().providesContacts = true;

            // now for the simplex generation
            StartCoroutine(RunSimplexMath(position, newBlock));
        }

        /// <summary>
        /// Runs the math to create the heights array asynchronously. This time,
        /// in the Unity way by using a coroutine.
        /// </summary>
        /// <param name="position">The position of the terrain cell in-world.</param>
        /// <returns>...what do i even put here? I guess it starts the task for the math, and waits until the task says it's completed, before storing the result in the tempHeightsArray field.</returns>
        private IEnumerator RunSimplexMath(Vector3 position, Terrain terrainBlock)
        {
            // timing numbers for debug operations
            long time = System.DateTime.Now.Ticks;
            long totalTime = time;
            if (extensiveDebug)
            {
                Debug.Log($"[MasterTerrainGenerator] Starting simplex math for cell {position}...");
            }

            // setup for and the simplex noise math
            Vector3 cellPosition = position / terrainCellSize + new Vector3(centerCellCoordinates.x, 0, centerCellCoordinates.y);
            CancellationTokenSource tokenSource = new();
            CancellationToken token = tokenSource.Token;
            Task<PerlinResultData> math = Task.Run(() => noiseGenerator.GenerateCellHeights(terrainCellSize, (int)cellPosition.x, (int)cellPosition.z, token));
            yield return new WaitUntil(() =>
            {
                return math.IsCompleted || math.IsCanceled;
            });

            if (extensiveDebug)
            {
                totalTime = System.DateTime.Now.Ticks - time;
                PrintPhaseTiming(totalTime, "Simplex math completed");
            }

            // we set some data onto the terrain object
            tempHeightsArray = math.Result.heights;
            terrainBlock.terrainData.SetHeights(0, 0, tempHeightsArray);
            ChunkData biomeData = terrainBlock.gameObject.AddComponent<ChunkData>();
            biomeData.SetValues(
                cellPosition, 
                math.Result.forestationFactor, 
                math.Result.dryFactor);

            // just to prevent too much processing in one frame
            yield return new WaitForFixedUpdate();
            FindAndConnectNeighbors(terrainBlock); // honestly probably doesnt take long enough to benchmark, at least not a significant time expenditure

            // paths n shits
            time = System.DateTime.Now.Ticks;
            if (pathsActive) pathGenerator.GeneratePathsFor(terrainBlock.gameObject);
            if (extensiveDebug)
            {
                totalTime = System.DateTime.Now.Ticks - time;
                PrintPhaseTiming(totalTime, "Paths completed");
            }
            yield return new WaitForFixedUpdate();

            // foliage features
            time = System.DateTime.Now.Ticks;
            if (featuresActive) featureGenerator.GenerateObjectsFor(terrainBlock.gameObject);
            if (extensiveDebug)
            {
                totalTime = System.DateTime.Now.Ticks - time;
                PrintPhaseTiming(totalTime, "Foliage features completed");
            }
            yield return new WaitForFixedUpdate();

            // path cleanup
            time = System.DateTime.Now.Ticks;
            if (pathsActive) pathGenerator.CleanupPathsFor(terrainBlock.gameObject);
            if (extensiveDebug)
            {
                totalTime = System.DateTime.Now.Ticks - time;
                PrintPhaseTiming(totalTime, "Paths cleaned up");
            }
            yield return new WaitForFixedUpdate();

            generating = false;
        }

        /// <summary>
        /// Helper method for printing debug timing per generation phase.
        /// </summary>
        /// <param name="timeTaken">How many ticks long the phase was, using DateTime.Ticks.</param>
        /// <param name="phaseMessage">The message describing this phase</param>
        private void PrintPhaseTiming(long timeTaken, string phaseMessage)
        {
            double milliseconds = timeTaken / 1000.0;
            if (milliseconds < 16.6) Debug.Log($"[MasterTerrainGenerator] {phaseMessage} in {milliseconds} ms (nice)");
            else if (milliseconds < 100.0) Debug.Log($"[MasterTerrainGenerator] {phaseMessage} in {milliseconds} ms");
            else if (milliseconds < 500.0) Debug.LogWarning($"[MasterTerrainGenerator] {phaseMessage} in {milliseconds} ms, quite long");
            else Debug.LogError($"[MasterTerrainGenerator] {phaseMessage} in {milliseconds} ms, exceptionally long");
        }

        /// <summary>
        /// Searches for and sets the neighbors for all nearby blocks to the current block.
        /// </summary>
        /// <param name="current">The current terrain block to neighborize.</param>
        private void FindAndConnectNeighbors(Terrain current)
        {
            // im thinking raycast for all 4 nearby squares to see if they need to be connected
            bool left = Physics.Raycast(current.transform.position + new Vector3(-terrainCellSize + 1, 100, 1), Vector3.down, out RaycastHit hit, 100, LayerMask.NameToLayer("Terrain"));
            if (left)
            {
                Terrain leftNeighbor = hit.transform.GetComponent<Terrain>();
                leftNeighbor.SetNeighbors(leftNeighbor.leftNeighbor, leftNeighbor.topNeighbor, current, leftNeighbor.bottomNeighbor);
                current.SetNeighbors(leftNeighbor, current.topNeighbor, current.rightNeighbor, current.bottomNeighbor);
            }

            bool top = Physics.Raycast(current.transform.position + new Vector3(1, 100, terrainCellSize + 1), Vector3.down, out hit, 100, LayerMask.NameToLayer("Terrain"));
            if (top)
            {
                Terrain topNeighbor = hit.transform.GetComponent<Terrain>();
                topNeighbor.SetNeighbors(topNeighbor.leftNeighbor, topNeighbor.topNeighbor, topNeighbor.rightNeighbor, current);
                current.SetNeighbors(current.leftNeighbor, topNeighbor, current.rightNeighbor, current.bottomNeighbor);
            }

            bool right = Physics.Raycast(current.transform.position + new Vector3(terrainCellSize + 1, 100, 1), Vector3.down, out hit, 100, LayerMask.NameToLayer("Terrain"));
            if (right)
            {
                Terrain rightNeighbor = hit.transform.GetComponent<Terrain>();
                rightNeighbor.SetNeighbors(current, rightNeighbor.topNeighbor, rightNeighbor.rightNeighbor, rightNeighbor.bottomNeighbor);
                current.SetNeighbors(current.leftNeighbor, current.topNeighbor, rightNeighbor, current.bottomNeighbor);
            }

            bool bottom = Physics.Raycast(current.transform.position + new Vector3(1, 100, -terrainCellSize + 1), Vector3.down, out hit, 100, LayerMask.NameToLayer("Terrain"));
            if (bottom)
            {
                Terrain bottomNeighbor = hit.transform.GetComponent<Terrain>();
                bottomNeighbor.SetNeighbors(bottomNeighbor.leftNeighbor, current, bottomNeighbor.rightNeighbor, bottomNeighbor.bottomNeighbor);
                current.SetNeighbors(current.leftNeighbor, current.topNeighbor, current.rightNeighbor, bottomNeighbor);
            }
        }
        #endregion
    }
}
// 101 SLOC