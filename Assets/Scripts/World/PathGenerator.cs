using System.Collections.Generic;
using UnityEngine;

namespace Permafrost.World
{
    #region Path definition
    /// <summary>
    /// Structure containing all relevant data relating to an in-world path.
    /// Mainly, includes the cell location, the direction the path moves in,
    /// any points in the path for the current chunk, and some metadata such 
    /// as how many paths have forked off of this one, the radius of each path
    /// point in-world, and whether the path has been marked for death (the
    /// path has ended and the object needs to be deleted at the end of this chunk
    /// generation step).
    /// </summary>
    public struct Path
    {
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

        /// <summary>
        /// Adds a point to the current list of path points this chunk.
        /// </summary>
        /// <param name="point">The point to add to the path.</param>
        public void AddPoint(Vector2 point)
        {
            currentPathPoints.Add(point);
        }

        /// <summary>
        /// Clears the path's points for this chunk, and updates the current cell
        /// to the new location. Note that both the new location and the converted
        /// location of the latest path point must be provided, and are not calculated
        /// here.
        /// </summary>
        /// <param name="cellLocation">The cell's x and y coordinate this path is entering.</param>
        /// <param name="convertedLastPathPoint">The latest point in this path, with its position adjusted to be relative to the new cell it is entering.</param>
        public void MoveToNewCell(Vector2 cellLocation, Vector2 convertedLastPathPoint)
        {
            currentPathPoints.Clear();
            currentPathPoints.Add(convertedLastPathPoint);
            currentCellLocation = cellLocation;
        }

        /// <summary>
        /// Returns the latest point in this path for the current cell.
        /// </summary>
        /// <returns>The latest path point for this cell.</returns>
        public readonly Vector2 GetLatestPoint()
        {
            return currentPathPoints[^1];
        }
    }
    #endregion

    /// <summary>
    /// Generates the paths that show up in-world. This involves two different phases;
    /// the first is updating the underlying path data, and the second is updating the
    /// current chunk heights and removing objects that may be overlapping the path.
    /// </summary>
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
        [SerializeField] private int pathTextureLayerIndex;

        [Header("Other Settings")]
        [Tooltip("Every looping operation, if more than this many loops happen, then the generation step ends and the next step begins.")]
        [SerializeField] private int maxLoopCountDuringGenerationSteps = 1000;
        [Tooltip("When paths enter their cleanup phase, this layer mask is used to determine what objects overlapping the path need to be destroyed.")]
        [SerializeField] private LayerMask objectsToClearWhenOnPaths;
        [Tooltip("When paths enter their cleanup pahse, this is the max number of objects that will be deleted if overlapping any given path point.")]
        [SerializeField] private int maxObjectsToDeleteDuringClear = 5;
        [SerializeField] private float heightRaycastDistance;

        private MasterTerrainGenerator masterGenerator;
        private Path centerRoad; // exclusively the only path that never ends
        private List<Path> activePaths; // todo: benchmark? unsure if the default implementation is linked list or not
        private List<Path> newlyFormedPaths; //  yep

        private Collider[] overlapBuffer;

        [Header("Debug")]
        [SerializeField] private bool debugEnabled;
        #endregion

        #region Unity Methods
        // setup
        private void Awake()
        {
            masterGenerator = GetComponent<MasterTerrainGenerator>();
            overlapBuffer = new Collider[maxObjectsToDeleteDuringClear];
            activePaths = new List<Path>(maxActivePathsCount);
            newlyFormedPaths = new List<Path>();
        }
        #endregion

        #region Generation Phase 1
        /// <summary>
        /// PHASE ONE: Generate paths and roads in the map.
        /// </summary>
        /// <param name="terrain">The GameObject for the current terrain cell.</param>
        public void GeneratePathsFor(GameObject terrain)
        {
            // i alias things because good god i dont like typing paragraphs
            // but also redoing some of these calculations on the fly is gonna be slower than just doing them once,
            // especially the number of times they may need to happen
            Bounds cellBounds = new(
                terrain.transform.position + new Vector3(masterGenerator.TerrainCellSize / 2, 0, masterGenerator.TerrainCellSize / 2),
                new Vector3(masterGenerator.TerrainCellSize, 1, masterGenerator.TerrainCellSize));
            System.Random RNG = masterGenerator.ObjectRNG;
            ChunkData dat = terrain.GetComponent<ChunkData>();
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
            int expectedPointsInCell = (int)(1 + cellBounds.size.x + factor1 + factor2);
            int pointsBetweenForks = expectedPointsInCell / pathForkAttemptsPerBlock;

            // if this is the center chunk, we only make the starter road
            if (dat.CellLocation.x == masterGenerator.CenterCellLocation.x && dat.CellLocation.z == masterGenerator.CenterCellLocation.y)
            {
                centerRoad = new Path(
                    4, 
                    dat.CellLocation, 
                    new Vector2(cellBounds.center.x, cellBounds.center.z), 
                    RNG.NextDouble() > 0.5f ? Vector2.left : Vector2.down);
            }
            ProliferateCenterRoad(terrain, cellBounds, RNG, dat, pointsBetweenForks);

            // proliferate all paths
            foreach (Path p in activePaths)
            {
                ProliferatePath(terrain, cellBounds, RNG, dat, pointsBetweenForks, p);
            }
            // and then proliferate the new paths that can't fork yet
            foreach (Path p in newlyFormedPaths)
            {
                ProliferatePathUnforking(terrain, cellBounds, RNG, dat, p);
            }
            foreach (Path p in newlyFormedPaths)
            {
                activePaths.Add(p);
            }
            newlyFormedPaths.Clear();
        }

        /// <summary>
        /// Expands the center road into the current chunk; assuming the center road is in this chunk
        /// </summary>
        /// <param name="terrain">The gameobject for the current terrain cell.</param>
        /// <param name="cellBounds">A Bounds representing the area of the cell.</param>
        /// <param name="RNG">The Master Terrain Generator's Object RNG.</param>
        /// <param name="dat">The ChunkData attached to the given terrain cell.</param>
        /// <param name="forkDistance">The number of path points between fork attempts.</param>
        private void ProliferateCenterRoad(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat, int forkDistance) 
        {
            // using && here to make the road go in both directions if possible
            if (dat.CellLocation.x != centerRoad.currentCellLocation.x && dat.CellLocation.z != centerRoad.currentCellLocation.y) return;

            Vector2 cur = centerRoad.GetLatestPoint();
            int i = 1;
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

                // fork roll
                if (i % forkDistance == 0)
                {
                    // mostly to ensure that we only do the inner rng roll when checks are needed
                    bool roll = RNG.NextDouble() < chanceToForkPaths;
                    if (roll) ForkCenterRoad(terrain, cellBounds, RNG, dat);
                }
                i++;
            } 
            while ((cur.x < cellBounds.size.x && cur.x > 0 && cur.y < cellBounds.size.z && cur.y > 0) || i < 5);
            // the i < 5 is a somewhat arbitrary number, but it handles the case that due to a random walk, the path
            // moves back to one of the edge coordinates without ever leaving the edge coordinate of a chunk. 5 is the
            // arbitrary part, i havent really done any math to figure out how often this would occur, but the chance
            // that any given path rolls 5 times backwards is wild. it's possible that a path could curve around to a
            // border, but this is the price im paying to make it work more reliably.
        }

        /// <summary>
        /// Creates a new path forked off the center road.
        /// </summary>
        /// <param name="terrain">The gameobject for the current terrain cell.</param>
        /// <param name="cellBounds">A Bounds representing the area of the cell.</param>
        /// <param name="RNG">The Master Terrain Generator's Object RNG.</param>
        /// <param name="dat">The ChunkData attached to the given terrain cell.</param>
        private void ForkCenterRoad(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat)
        {
            // using && here to make the road go in both directions if possible
            if (dat.CellLocation.x != centerRoad.currentCellLocation.x && dat.CellLocation.z != centerRoad.currentCellLocation.y) return;
            if (activePaths.Count >= maxActivePathsCount) return;

            Path newPath = new(
                2, // radius
                centerRoad.currentCellLocation,
                centerRoad.GetLatestPoint(),
                new Vector2(
                    (float)(RNG.NextDouble() * 2 - 1),
                    (float)(RNG.NextDouble() * 2 - 1)
                ).normalized
            );
            activePaths.Add(newPath);
            // note center road doesnt have its fork count incremented cause it never ends so we dont really give a fuck
        }

        /// <summary>
        /// Expands the provided path through the current chunk. Assuming that path is in the current chunk.
        /// </summary>
        /// <param name="terrain">The gameobject for the current terrain cell.</param>
        /// <param name="cellBounds">A Bounds representing the area of the cell.</param>
        /// <param name="RNG">The Master Terrain Generator's Object RNG.</param>
        /// <param name="dat">The ChunkData attached to the given terrain cell.</param>
        /// <param name="forkDistance">The number of path points between fork attempts.</param>
        /// <param name="path">The Path being proliferated.</param>
        private void ProliferatePath(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat, int forkDistance, Path path)
        {
            if (dat.CellLocation.x != path.currentCellLocation.x || dat.CellLocation.z != path.currentCellLocation.y) return;
            if (path.markedForDeath) return;

            Vector2 cur = path.GetLatestPoint();
            int i = 1;
            float x = cur.x, y = cur.y;
            do
            {
                // progress path
                if (RNG.NextDouble() >= randomWalkChance)
                {
                    // here we have to do some funky math
                    x += path.direction.x;
                    y += path.direction.y;
                }
                else
                {
                    // random walker
                    x += path.direction.x + RNG.Next(-1, 1);
                    y += path.direction.y + RNG.Next(-1, 1);
                }
                cur = new(Mathf.Round(x), Mathf.Round(y));
                path.AddPoint(cur);

                // fork roll
                if (i % forkDistance == 0)
                {
                    // mostly to ensure that we only do the inner rng roll when checks are needed
                    bool roll = RNG.NextDouble() < chanceToForkPaths;
                    if (roll) ForkPath(terrain, cellBounds, RNG, dat, path);
                }
                // kill roll
                if (path.forks >= minimumForksPerPath)
                {
                    bool roll = RNG.NextDouble() < chanceToEndPaths;
                    if (roll)
                    {
                        path.markedForDeath = true;
                        return;
                    }
                }
                i++;
            }
            while ((cur.x < cellBounds.size.x && cur.x > 0 && cur.y < cellBounds.size.z && cur.y > 0) || i < 5);
        }

        /// <summary>
        /// Forks a path into a new one.
        /// </summary>
        /// <param name="terrain">The gameobject for the current terrain cell.</param>
        /// <param name="cellBounds">A Bounds representing the area of the cell.</param>
        /// <param name="RNG">The Master Terrain Generator's Object RNG.</param>
        /// <param name="dat">The ChunkData attached to the given terrain cell.</param>
        /// <param name="original">The Path being proliferated.</param>
        private void ForkPath(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat, Path original)
        {
            if (dat.CellLocation.x != original.currentCellLocation.x || dat.CellLocation.z != original.currentCellLocation.y) return;
            if (activePaths.Count >= maxActivePathsCount) return;

            Path newPath = new(
                2, // radius
                original.currentCellLocation,
                original.GetLatestPoint(),
                new Vector2(
                    (float)(RNG.NextDouble() * 2 - 1),
                    (float)(RNG.NextDouble() * 2 - 1)
                ).normalized
            );
            newlyFormedPaths.Add(newPath);
            original.forks++;
        }

        /// <summary>
        /// Proliferates a path, assuming this is the first chunk a path is in and it doesn't need to fork.
        /// </summary>
        /// <param name="terrain">The gameobject for the current terrain cell.</param>
        /// <param name="cellBounds">A Bounds representing the area of the cell.</param>
        /// <param name="RNG">The Master Terrain Generator's Object RNG.</param>
        /// <param name="dat">The ChunkData attached to the given terrain cell.</param>
        /// <param name="path">The Path being proliferated.</param>
        private void ProliferatePathUnforking(GameObject terrain, Bounds cellBounds, System.Random RNG, ChunkData dat, Path path) 
        {
            if (dat.CellLocation.x != path.currentCellLocation.x || dat.CellLocation.z != path.currentCellLocation.y) return;
            if (path.markedForDeath) return;

            Vector2 cur = path.GetLatestPoint();
            float x = cur.x, y = cur.y;
            int i = 1;
            do
            {
                // progress path
                if (RNG.NextDouble() >= randomWalkChance)
                {
                    // here we have to do some funky math
                    x += path.direction.x;
                    y += path.direction.y;
                }
                else
                {
                    // random walker
                    x += path.direction.x + RNG.Next(-1, 1);
                    y += path.direction.y + RNG.Next(-1, 1);
                }
                cur = new(Mathf.Round(x), Mathf.Round(y));
                path.AddPoint(cur);

                // fork and kill roll unneeded
                i++;
            }
            while ((cur.x < cellBounds.size.x && cur.x > 0 && cur.y < cellBounds.size.z && cur.y > 0) || i < 5);
        }
        #endregion

        #region Generation Phase 2
        /// <summary>
        /// PHASE THREE?: Cleanup portion of path generation. this is where we actually modify the terrain cell.
        /// </summary>
        /// <param name="terrain">The gameobject for the terrain cell to modify.</param>
        public void CleanupPathsFor(GameObject terrain)
        {
            Bounds cellBounds = new(
                terrain.transform.position + new Vector3(masterGenerator.TerrainCellSize / 2, 0, masterGenerator.TerrainCellSize / 2),
                new Vector3(masterGenerator.TerrainCellSize, 1, masterGenerator.TerrainCellSize));
            float[,] heights = terrain.GetComponent<Terrain>().terrainData.GetHeights(0, 0, masterGenerator.TerrainCellSize, masterGenerator.TerrainCellSize);
            float[,,] alphas = terrain.GetComponent<Terrain>().terrainData.GetAlphamaps(0, 0, masterGenerator.TerrainCellSize, masterGenerator.TerrainCellSize);
            ChunkData dat = terrain.GetComponent<ChunkData>();

            // first, center road
            CleanupPath(terrain, cellBounds, dat, heights, alphas, centerRoad);

            // finally, update the terrain heights + textures
            terrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heights);
            terrain.GetComponent<Terrain>().terrainData.SetAlphamaps(0, 0, alphas);
        }

        /// <summary>
        /// Does cleanup, removing objects placed on the given path and setting the corresponding terrain data for this chunk.
        /// </summary>
        /// <param name="terrain">The gameobject for the current terrain cell.</param>
        /// <param name="cellBounds">A bounds representing the area of the terrain cell.</param>
        /// <param name="dat">The ChunkData related to/attached to the terrain.</param>
        /// <param name="heights">The heights array for the current terrain cell.</param>
        /// <param name="alphas">The alphamap/textures array for the current terrain cell.</param>
        /// <param name="path">The current path to be updated.</param>
        private void CleanupPath(GameObject terrain, Bounds cellBounds, ChunkData dat, float[,] heights, float[,,] alphas, Path path)
        {
            if (dat.CellLocation.x != path.currentCellLocation.x || dat.CellLocation.z != path.currentCellLocation.y) return;

            // start with the overlap sphering
            foreach (Vector2 point in path.currentPathPoints)
            {
                //int i = Physics.OverlapSphereNonAlloc(point, path.radius, overlapBuffer, objectsToClearWhenOnPaths, QueryTriggerInteraction.Ignore);
                int i = Physics.OverlapCapsuleNonAlloc(new Vector3(point.x, 0, point.y), new Vector3(point.x, heightRaycastDistance, point.y), path.radius, overlapBuffer, objectsToClearWhenOnPaths, QueryTriggerInteraction.Ignore);
                for (; i > 0; i--)
                {
                    Destroy(overlapBuffer[i - 1].gameObject);
                }
            }

            int min = (int)Mathf.Max(-path.radius, 0);
            int max = (int)Mathf.Min(path.radius, cellBounds.size.x);
            // now we modify the terrain n textures
            foreach (Vector2 point in path.currentPathPoints)
            {
                for (int i = min; i < max; i++)
                {
                    for (int j = min; j < max; j++)
                    {
                        heights[(int)point.x + i, (int)point.y + j] -= 0.00025f;
                        alphas[(int)point.x + i, (int)point.y + j, pathTextureLayerIndex] = 0.5f;
                        alphas[(int)point.x + i, (int)point.y + j, 0] = 0;
                    }
                }
            }
        }
        #endregion
    }
}