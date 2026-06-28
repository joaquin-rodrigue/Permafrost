using System.Collections.Generic;
using UnityEngine;

namespace Permafrost.World
{
    public struct Path
    {
        public bool active;
        public bool markedForDeath;
        public float radius;
        public int forks;
        public Vector2 currentCellLocation;
        public List<Vector2> currentPathPoints;
        public Vector2 direction;

        /// <summary>
        /// Makes a path using the provided radius, setting just one point and the provided direction.
        /// </summary>
        public Path(float radius, Vector2 cellLocation, Vector2 firstPoint, Vector2 direction)
        {
            active = true;
            markedForDeath = false;
            this.radius = radius;
            forks = 0;

            currentCellLocation = cellLocation;
            currentPathPoints = new List<Vector2>
            {
                firstPoint
            };
            this.direction = direction;
        }

        public void AddPoint(Vector2 point)
        {
            currentPathPoints.Add(point);
        }

        public void MoveToNewCell(Vector2 cellLocation, Vector2 convertedLastPathPoint)
        {
            currentPathPoints.Clear();
            currentPathPoints.Add(convertedLastPathPoint);
            currentCellLocation = cellLocation;
        }

        public Vector2 GetLatestPoint()
        {
            return currentPathPoints[currentPathPoints.Count - 1];
        }
    }

    [RequireComponent(typeof(MasterTerrainGenerator))]
    public class PathGenerator : MonoBehaviour
    {
        #region Data
        [Header("Paths Config")]
        [Tooltip("The maximum number of paths that can simulatneously exist at one time. If a path ends, it is no longer active, but otherwise, it remains active until the end of the path is reached.")]
        [Range(1, 1000)]
        [SerializeField] private int maxActivePathsCount;
        [Tooltip("In every block, this chance is rolled pathForkAttemptsPerBlock times to fork a path into a new one.")]
        [Range(0f, 1f)]
        [SerializeField] private float chanceToForkPaths;
        [Tooltip("How many times to attempt path forking per block, see chanceToForkPaths.")]
        [SerializeField] private int pathForkAttemptsPerBlock;
        [Tooltip("The chance for any given path to become inactive in a block and end.")]
        [Range(0f, 1f)]
        [SerializeField] private float chanceToEndPaths;
        [Tooltip("How many times a path must fork before it can end. Paths may fork more than this many times, but never less.")]
        [Range(1, 1000)]
        [SerializeField] private int minimumForksPerPath;
        [Tooltip("The chance for a path to randomly walk in a diagonal away from its given direction.")]
        [Range(0f, 1f)]
        [SerializeField] private float randomWalkChance;

        [Header("Other Settings")]
        [Tooltip("Every looping operation, if more than this many loops happen, then the generation step ends and the next step begins.")]
        [SerializeField] private int maxLoopCountDuringGenerationSteps = 1000;
        [SerializeField] private LayerMask terrainLayerMask;
        [SerializeField] private float heightRaycastDistance;

        private MasterTerrainGenerator masterGenerator;
        private Path centerRoad; // exclusively the only path that never ends
        private List<Path> activePaths;

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

        #region Generation Phase
        /// <summary>
        /// PHASE ONE: Generate paths and roads in the map.
        /// </summary>
        /// <param name="terrain">The GameObject for the current terrain cell.</param>
        public void GeneratePathsFor(GameObject terrain)
        {
            // i alias things because good god i dont like typing paragraphs
            Bounds cellBounds = new(
                terrain.transform.position + new Vector3(masterGenerator.TerrainCellSize / 2, 0, masterGenerator.TerrainCellSize / 2),
                new Vector3(masterGenerator.TerrainCellSize, 1, masterGenerator.TerrainCellSize));
            System.Random RNG = masterGenerator.ObjectRNG;
            ChunkData dat = terrain.GetComponent<ChunkData>();

            // if this is the center chunk, we only make the starter road
            if (dat.CellLocation.x == masterGenerator.CenterCellLocation.x && dat.CellLocation.z == masterGenerator.CenterCellLocation.y)
            {
                centerRoad = new Path(
                    4, 
                    dat.CellLocation, 
                    new Vector2(cellBounds.center.x, cellBounds.center.z), 
                    RNG.NextDouble() > 0.5f ? Vector2.left : Vector2.down);
            }
            ProliferateCenterRoad(terrain, cellBounds, RNG, dat);
        }

        /// <summary>
        /// Expands the center road into the current chunk; assuming the center road is in this chunk
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="cellBounds"></param>
        /// <param name="RNG"></param>
        /// <param name="dat"></param>
        private void ProliferateCenterRoad(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat) 
        {
            // using && here to make the road go in both directions if possible
            if (dat.CellLocation.x != centerRoad.currentCellLocation.x && dat.CellLocation.z != centerRoad.currentCellLocation.y) return;

            Vector2 cur = centerRoad.GetLatestPoint();
            do
            {
                // progress path
                if (RNG.NextDouble() >= randomWalkChance)
                {
                    // normal case: just proceed in direction
                    cur += centerRoad.direction;
                    centerRoad.AddPoint(cur);
                }
                else
                {
                    // funky case: random walk
                    // its not quite a random walk but it's close enough and it can cause some funky
                    cur += new Vector2(
                        centerRoad.direction.x + RNG.Next(-1, 1),
                        centerRoad.direction.y + RNG.Next(-1, 1)
                    );
                    centerRoad.AddPoint(cur);
                }
            } while (cur.x < cellBounds.size.x && cur.x > 0 && cur.y < cellBounds.size.z && cur.y > 0);
        }

        private void ForkCenterRoad(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat)
        {
            // using && here to make the road go in both directions if possible
            if (dat.CellLocation.x != centerRoad.currentCellLocation.x && dat.CellLocation.z != centerRoad.currentCellLocation.y) return;

            // so based on my math:
            // 1. average case scenario: random walks will equalize to be the same as the given path direction on average,
            // so since the cells actually have 1 more integer coordinate for terrain heights than the given width, we 
            // have 1 + cellBounds.size (which is just terrain cell size)
            // 2. shortest case scenario: every random walk roll will move in the same direction as the path, shortening it
            // by 1 for each roll, which will happen on average randomWalkChance * cell size times per cell
            // 3. longest case scenario: every random walk roll will move in the opposite direction as the path, which makes
            // the path one longer, allowing more chances to roll backwards, which as far as I can tell is quadratic through
            // a few manual calculations, kinda the opposite of a random walk deviating on average the square root of the 
            // walk's length away from the origin
            float factor1 = (cellBounds.size.x * randomWalkChance) * (cellBounds.size.x * randomWalkChance);
            float factor2 = -(cellBounds.size.x * randomWalkChance);
            int expectedPointsInCell = (int) (1 + cellBounds.size.x + factor1 + factor2);
        }

        private void ProliferateAllPaths(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat)
        {

        }
        #endregion
    }
}