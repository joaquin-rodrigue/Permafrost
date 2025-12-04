using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Generates pseudorandom terrain using a Perlin Noise algorithm.<br></br>
///     Still in development lol<br></br>
///     Currently might be in need of some optimization; the average chunk generation time is ~300-350 ms
/// </summary>
public class RandomTerrainGenerator : MonoBehaviour
{
    private List<Vector2> terrains;
    private byte[] permutation;
    private byte[] p;

    public System.Random random { get; private set; }
    public Bounds MapBounds { get; set; }

    [Tooltip("Leave as '0' for random")]
    [SerializeField] private int seed;
    [SerializeField] private int terrainCellSize;
    [SerializeField] private RandomSpawner objectSpawner;
    [SerializeField] private TerrainLayer mat;
    [SerializeField] private Material mat2;


    // Starts the generation and stuff
    void Start()
    {
        terrains = new List<Vector2>();
        Debug.LogWarning(mat.diffuseTexture);
        if (seed == 0)
        {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        
        // Make some noise
        GenerateNoise(seed);
        long time = DateTime.Now.Ticks;
        try
        {
            Invoke(nameof(EmergencyCancel), 60f);
            GenerateThreeGrid();
            CancelInvoke(nameof(EmergencyCancel));
        }
        catch (Exception e)
        {
            Debug.LogError("Exception during world gen: " + e.Message);
            Debug.LogError(e.StackTrace);
            Debug.LogError("Aborting world gen");
        }
        long done = DateTime.Now.Ticks;
        Debug.Log("Generated in " + ((done - time) / 1000f) + " ms");

        // Place starter objects
        GameObject player = GameObject.Find("Player");
        RaycastHit chosenPoint = new();
        Physics.Raycast(player.transform.position + new Vector3(0, 100, 0), Vector3.down, out chosenPoint);
        player.transform.position = chosenPoint.point + Vector3.up;

        GameObject car = GameObject.Find("Car");
        Physics.Raycast(car.transform.position + new Vector3(0, 100, 0), Vector3.down, out chosenPoint);
        car.transform.position = chosenPoint.point + Vector3.up * 2;

        GameObject axe = GameObject.Find("Axe");
        Physics.Raycast(axe.transform.position + new Vector3(0, 100, 0), Vector3.down, out chosenPoint);
        axe.transform.position = chosenPoint.point + Vector3.up;
    }

    public void GenerateNewChunk(Vector2 pos)
    {
        Vector2 cell = pos / terrainCellSize;
        Vector2 temp2 = new Vector2(Mathf.Floor(cell.x), Mathf.Floor(cell.y)) * terrainCellSize;
        
        foreach (Vector2 existing in terrains)
        {
            if (existing.Equals(temp2))
            {
                return;
            }
        }
        Debug.Log("working until generate" + temp2);
        GenerateNewBlock(temp2);
    }

    #region Grid Blocks
    /// <summary>
    /// Generates a grid of 15x15 terrain cells.
    /// </summary>
    private void GenerateFifteenGrid()
    {
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                GenerateNewBlock(new Vector2(i, j) * terrainCellSize);
            }
        }
    }

    /// <summary>
    /// Generates a grid of 3x3 terrain cells. This should be centered around the 0,0 terrain cell instead of just... going wherever.
    /// </summary>
    private void GenerateThreeGrid()
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                GenerateNewBlock(new Vector2(i, j) * terrainCellSize);
            }
        }
    }

    /// <summary>
    /// Generates a new terrain cell.
    /// </summary>
    /// <param name="pos">The position in Unity world coordinates to place the terrain. This should be a multiple of terrainCellSize.</param>
    private void GenerateNewBlock(Vector2 pos)
    {
        long time = DateTime.Now.Ticks;
        TerrainData data = new();
        TerrainLayer[] layers = new TerrainLayer[1];
        layers[0] = mat;
        data.terrainLayers = layers;
        //Debug.Log(data.terrainLayers[0]);
        Terrain newBlock = Terrain.CreateTerrainGameObject(data).GetComponent<Terrain>();
        newBlock.transform.position = new Vector3(pos.x, 0, pos.y);
        //Debug.Log(newBlock.terrainData.terrainLayers[0]);
        //Debug.Log(newBlock.terrainData.terrainLayers[0].diffuseTexture);
        newBlock.materialTemplate = mat2;
        Debug.Log(newBlock.materialTemplate);
        newBlock.gameObject.layer = LayerMask.NameToLayer("Terrain");
        newBlock.GetComponent<TerrainCollider>().providesContacts = true;
        newBlock.terrainData.heightmapResolution = terrainCellSize + 1;
        newBlock.terrainData.size = new Vector3(terrainCellSize, 600, terrainCellSize);
        newBlock.terrainData.SetDetailResolution(128, 32);
        //newBlock.allowAutoConnect = true;

        MapBounds = new(new Vector3(pos.x + terrainCellSize / 2, 0, pos.y + terrainCellSize / 2), new Vector3(terrainCellSize, 0, terrainCellSize));
        //Debug.Log(MapBounds);
        //Debug.Log(pos);
        terrains.Add(pos);

        Vector2 cell = pos / terrainCellSize + new Vector2(31, 31);
        //Debug.Log(cell);
        float[,] heights = new float[terrainCellSize + 1, terrainCellSize + 1];
        for (int i = 0; i < terrainCellSize + 1; i++)
        {
            for (int j = 0; j < terrainCellSize + 1; j++)
            {
                heights[i, j] = 0.15f * Perlin(cell.x + (j / (float) terrainCellSize), cell.y + (i / (float) terrainCellSize))
                    + 0.075f * Perlin(cell.x * 2 + (j / (float) terrainCellSize * 2f), cell.y * 2 + (i / (float) terrainCellSize * 2f));
            }
        }

        // The middle road
        if (cell.x == 31)
        {
            for (int i = 0; i < terrainCellSize + 1; i++)
            {
                float left = heights[i, terrainCellSize / 2 - 4];
                float right = heights[i, terrainCellSize / 2 + 4];
                float roadHeight = (left + right) / 2 - 0.0015f;
                for (int j = terrainCellSize / 2 - 4; j < terrainCellSize / 2 + 4; j++)
                {
                    heights[i, j] = roadHeight;
                }
            }
        }

        newBlock.terrainData.SetHeights(0, 0, heights);
        //newBlock.terrainData.SetBaseMapDirty();
        //Debug.LogWarning(newBlock.terrainData.terrainLayers[0]);
        FindAndConnectNeighbors(newBlock);
        objectSpawner.Generate();
        long done = DateTime.Now.Ticks;
        Debug.Log("Generated block " + cell + " in " + ((done - time) / 1000f) + " ms");

    }
    #endregion

    /// <summary>
    /// In case generation is, for some reason, taking longer than a minute (it's never taken more than a few seconds) this throws an exception to stop it.
    /// </summary>
    private void EmergencyCancel()
    {
        throw new TimeoutException("Generation time exceeded 60 seconds - something has gone wrong");
    }

    /// <summary>
    /// Generates the permutation table for noise generation. This is generated randomly based on the seed provided.
    /// </summary>
    /// <param name="seed">The seed for the random object. Should be provided by the object's seed property.</param>
    private void GenerateNoise(int seed)
    {
        random = new(seed);

        // Make a permutation table
        permutation = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            permutation[i] = (byte) random.Next(0, 255);
        }
        p = new byte[512];
        for (int i = 0; i < 512; i++)
        {
            p[i] = permutation[i % 256];
        }
    }

    #region Perlin Noise Generation
    // https://adrianb.io/2014/08/09/perlinnoise.html
    // My understanding is limited here but I'll comment for my future understanding hopefully
    private float Perlin(float x, float y)
    {
        // Separate int and float portions of the coords
        int xi = (int) x, yi = (int) y;
        float xf = x - xi, yf = y - yi;

        // Fade function coefficients?
        float u = PerlinFade(xf), v = PerlinFade(yf);

        // Get the gradient vector offsets via this silly hash function magic
        byte aa, ab, ba, bb;
        aa = p[p[xi] + yi];
        ab = p[p[xi] + yi + 1];
        ba = p[p[xi + 1] + yi];
        bb = p[p[xi + 1] + yi + 1];

        // Lerp
        float lerp1, lerp2;
        lerp1 = Lerp(PerlinGradient(aa, xf, yf, 0), PerlinGradient(ba, xf - 1, yf, 0), u);
        lerp2 = Lerp(PerlinGradient(ab, xf, yf - 1, 0), PerlinGradient(bb, xf - 1, yf - 1, 0), u);
        return (Lerp(lerp1, lerp2, v) + 1) / 2;
    }

    // Linearly interpolates between two floats
    private float Lerp(float vec1, float vec2, float weight)
    {
        return vec1 + weight * (vec2 - vec1);
    }

    // this is the weird part because I just don't need the z component but I can't figure out for the life of me what this code looks like without it
    // Its basically a lookup table to make the gradient vectors
    private float PerlinGradient(int hash, float x, float y, float z)
    {
        return (hash & 0xF) switch
        {
            0x0 => x + y,
            0x1 => -x + y,
            0x2 => x - y,
            0x3 => -x - y,
            0x4 => x + z,
            0x5 => -x + z,
            0x6 => x - z,
            0x7 => -x - z,
            0x8 => y + z,
            0x9 => -y + z,
            0xA => y - z,
            0xB => -y - z,
            0xC => y + x,
            0xD => -y + z,
            0xE => y - x,
            0xF => -y - z,
            _ => 0,// never happens
        };
    }

    // Fade function for the perlin noise to smooth it out
    private float PerlinFade(float val)
    {
        return val * val * val * (val * (val * 6 - 15) + 10);
    }

    // Connects all the neighbor terrain cells
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
