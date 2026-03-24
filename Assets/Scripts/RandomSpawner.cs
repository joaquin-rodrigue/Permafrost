using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Generates objects in the world randomly.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    // Object refs
    [Header("Objects + Ranges")]
    [SerializeField] private GameObject animal;
    [SerializeField] private GameObject wolf;
    [SerializeField] private GameObject bigfoot;
    [SerializeField] private GameObject tree;
    [SerializeField] private GameObject bush;
    [SerializeField] private GameObject shack;
    [SerializeField] private GameObject snowfall;
    [SerializeField] private float maxSpawningDistance;
    [SerializeField] private float minSpawningDistance;

    // The chicken
    private float animalSpawnTime;
    private float animalSpawnTimer;
    private Stack<GameObject> animalPool = new();
    [Header("Animals")]
    [SerializeField] private int maxAnimalCount;
    [SerializeField] private float animalSpawnTimeMax;
    [SerializeField] private float animalSpawnTimeMin;

    // The wolf
    private float wolfSpawnTime;
    private float wolfSpawnTimer;
    private Stack<GameObject> wolfPool = new();
    [Header("Wolves")]
    [SerializeField] private int maxWolfCount;
    [SerializeField] private float wolfSpawnTimeMax;
    [SerializeField] private float wolfSpawnTimeMin;

    // The foot
    private float bigfootSpawnTime;
    private float bigfootSpawnTimer;
    private bool bigfootAlive;
    private GameObject activeBigfoot;
    [Header("Bigfoot")]
    [SerializeField] private float bigfootRespawnTime;
    [SerializeField] private float bigfootRespawnModifier;

    // The happy little trees + bushes
    private int treeCount;
    private int bushCount;
    [Header("Trees/Bushes")]
    [SerializeField] private int treeCountMax;
    [SerializeField] private int treeCountMin;
    [SerializeField] private int bushCountMax;
    [SerializeField] private int bushCountMin;

    // The shacks
    private int itemSpawnWeightsTotal;
    [Header("Shacks")]
    [SerializeField] private float shackSpawnChance;
    [SerializeField] private int[] itemSpawnWeights;
    [SerializeField] private GameObject[] itemsToSpawn;

    // Everything else
    [Header("Other")]
    [SerializeField] private RandomTerrainGenerator generation;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private int maxLoopCountDuringGeneration = 1000;
    [SerializeField] private int maxLoopCountDuringSpawning = 500;
    [SerializeField] private SaveData dataCollector;

    #region Initial Object Spawning
    // Start method - generates the object pools mainly
    private void Start()
    {
        bigfootSpawnTime = bigfootRespawnTime;
        

        Debug.Log("Generating object pools...");
        long time = DateTime.Now.Ticks;
        for (int i = 0; i < maxAnimalCount; i++)
        {
            GameObject newAnimal = Instantiate(animal);
            newAnimal.SetActive(false);
            animalPool.Push(newAnimal);
        }
        for (int i = 0; i < maxWolfCount; i++)
        {
            GameObject newWolf = Instantiate(wolf);
            newWolf.SetActive(false);
            wolfPool.Push(newWolf);
        }
        long now = DateTime.Now.Ticks;
        Debug.Log("Generated object pools in " + ((now - time) / 1000f) + " ms");
    }

    // Runs the code to spawn one-time objects
    public void Generate(GameObject terrainParent)
    {
        treeCount = generation.random.Next(treeCountMin, treeCountMax);
        bool foundValidSpawn = false;
        RaycastHit hit = new();

        // --- TREES ---
        int loopCount = 0; // prevent rng from causing an infinite loop
        for (int i = 0; i < treeCount && loopCount < maxLoopCountDuringGeneration;)
        {
            foundValidSpawn = false;
            while (!foundValidSpawn && loopCount < maxLoopCountDuringGeneration)
            {
                foundValidSpawn = Physics.Raycast(new Vector3(
                    generation.random.Next((int) generation.MapBounds.min.x, (int) generation.MapBounds.max.x), 
                    120, 
                    generation.random.Next((int) generation.MapBounds.min.z, (int) generation.MapBounds.max.z)), 
                    Vector3.down, 
                    out hit, 
                    120,
                    terrainLayerMask
                );
                loopCount++;
            }
            //Debug.Log(hit.point);

            if (loopCount > maxLoopCountDuringGeneration)
            {
                Debug.Log("trees: no spawns left!");
                break;
            }

            int countInArea = generation.random.Next(1, 10);

            for (int j = 0; j < countInArea; j++)
            {
                RaycastHit chosenPoint = new();
                // this is WILD
                while (!Physics.Raycast(new Vector3(
                    hit.point.x + generation.random.Next(-10, 10),
                    hit.point.y + 20,
                    hit.point.z + generation.random.Next(-10, 10)),
                    Vector3.down,
                    out chosenPoint,
                    50,
                    terrainLayerMask
                ) && loopCount < maxLoopCountDuringGeneration) loopCount++;
                GameObject current = Instantiate(tree, terrainParent.transform);
                current.transform.SetPositionAndRotation(chosenPoint.point, Quaternion.Euler(0, generation.random.Next(0, 360), 0));
                current.transform.localScale = new Vector3(UnityEngine.Random.value * 0.5f + 0.95f, UnityEngine.Random.value * 0.5f + 0.95f, UnityEngine.Random.value * 0.5f + 0.95f);
                i++;
            }
        }

        // --- BUSHES ---
        bushCount = generation.random.Next(bushCountMin, bushCountMax);
        loopCount = 0;
        for (int i = 0; i < bushCount && loopCount < maxLoopCountDuringGeneration;)
        {
            foundValidSpawn = false;
            while (!foundValidSpawn && loopCount < maxLoopCountDuringGeneration)
            {
                foundValidSpawn = Physics.Raycast(new Vector3(
                    generation.random.Next((int)generation.MapBounds.min.x, (int)generation.MapBounds.max.x),
                    120,
                    generation.random.Next((int)generation.MapBounds.min.z, (int)generation.MapBounds.max.z)),
                    Vector3.down,
                    out hit,
                    120,
                    terrainLayerMask
                );
                loopCount++;
            }

            if (loopCount > maxLoopCountDuringGeneration)
            {
                Debug.Log("bush: no spawns left!");
                break;
            }

            int countInArea = generation.random.Next(1, 3);

            for (int j = 0; j < countInArea; j++)
            {
                RaycastHit chosenPoint = new();
                // this is WILD
                while (!Physics.Raycast(new Vector3(
                    hit.point.x + generation.random.Next(-10, 10),
                    hit.point.y + 20,
                    hit.point.z + generation.random.Next(-10, 10)),
                    Vector3.down,
                    out chosenPoint,
                    50,
                    terrainLayerMask
                ) && loopCount < maxLoopCountDuringGeneration) loopCount++;
                GameObject current = Instantiate(bush, terrainParent.transform);
                current.transform.SetPositionAndRotation(chosenPoint.point, Quaternion.Euler(0, generation.random.Next(0, 360), 0));
                current.transform.localScale = new Vector3(UnityEngine.Random.value * 0.1f + 0.95f, UnityEngine.Random.value * 0.1f + 0.95f, UnityEngine.Random.value * 0.1f + 0.95f);
                i++;
            }
        }

        // --- SHACKS ---
        itemSpawnWeightsTotal = 0;
        foreach (int weight in itemSpawnWeights)
        {
            itemSpawnWeightsTotal += weight;
        }

        double roll = generation.random.NextDouble();
        if (roll < shackSpawnChance)
        {
            loopCount = 0;
            foundValidSpawn = false;
            while (!foundValidSpawn && loopCount < maxLoopCountDuringGeneration)
            {
                foundValidSpawn = Physics.Raycast(new Vector3(
                    generation.random.Next((int)generation.MapBounds.min.x, (int)generation.MapBounds.max.x),
                    120,
                    generation.random.Next((int)generation.MapBounds.min.z, (int)generation.MapBounds.max.z)),
                    Vector3.down,
                    out hit,
                    120,
                    terrainLayerMask
                );
                loopCount++;
            }

            if (loopCount > maxLoopCountDuringGeneration)
            {
                Debug.Log("shack: no spawns left!");
            }
            else
            {
                // now we actually spawn the shack and drop the items in
                GameObject newShack = Instantiate(shack, terrainParent.transform);
                newShack.transform.SetPositionAndRotation(hit.point + Vector3.up, Quaternion.Euler(0, generation.random.Next(0, 360), 0));
                int choice = generation.random.Next(itemSpawnWeightsTotal);
                int itemIndex = -1, temp = 0;
                //Debug.Log(choice);
                for (int i = 0; i < itemSpawnWeights.Length; i++)
                {
                    if (itemSpawnWeights[i] + temp > choice)
                    {
                        itemIndex = i;
                        break;
                    }
                    temp += itemSpawnWeights[i];
                }
                GameObject item = Instantiate(itemsToSpawn[itemIndex], newShack.transform);
                item.transform.SetPositionAndRotation(hit.point + new Vector3(0, 3, 0), Quaternion.identity);
            }
        }

        // --- ATTACH SNOWFALL ---
        GameObject snowfallParticles = Instantiate(snowfall, terrainParent.transform);
        snowfallParticles.transform.localPosition += new Vector3(0, 100, 0);
    }
    #endregion

    #region Object Spawning Over Time
    // Update is called once per frame
    void Update()
    {
        animalSpawnTimer += Time.deltaTime;
        if (animalSpawnTimer > animalSpawnTime)
        {
            SpawnAnimal();
        }

        wolfSpawnTimer += Time.deltaTime;
        if (wolfSpawnTimer > wolfSpawnTime)
        {
            SpawnWolf();
        }

        if (!bigfootAlive)
        {
            bigfootSpawnTimer += Time.deltaTime;
        }
        if (bigfootSpawnTimer > bigfootSpawnTime)
        {
            SpawnBigfoot();
        }
    }

    // Finds a valid place to spawn an animal/enemy
    private RaycastHit FindValidSpawnPoint()
    {
        bool foundValidSpawn = false;
        int loopCount = 0;
        RaycastHit hit = new();
        while (!foundValidSpawn && loopCount < maxLoopCountDuringSpawning)
        {
            foundValidSpawn = Physics.Raycast(
                new Vector3(
                    playerTransform.position.x + (UnityEngine.Random.Range(minSpawningDistance, maxSpawningDistance) * ((UnityEngine.Random.value > 0.5f) ? -1 : 1)),
                    maxSpawningDistance,
                    playerTransform.position.z + (UnityEngine.Random.Range(minSpawningDistance, maxSpawningDistance) * ((UnityEngine.Random.value > 0.5f) ? -1 : 1))
                ),
                Vector3.down,
                out hit,
                maxSpawningDistance,
                terrainLayerMask
            );
            loopCount++;
        }
        if (loopCount >= maxLoopCountDuringSpawning)
        {
            Debug.LogWarning("tried spawning an object but could not find a valid location!");
        }
        return hit;
    }

    // Spawns an animal somewhere at random
    private void SpawnAnimal()
    {
        if (animalPool.Count == 0)
        {
            Debug.LogWarning("no animals left!");
            return;
        }
        animalSpawnTimer = 0;

        RaycastHit hit = FindValidSpawnPoint();
        
        GameObject possible = animalPool.Pop();
        //Debug.Log(possible);
        if (possible != null)
        {
            possible.SetActive(true);
            //Debug.Log(hit.point);
            possible.transform.SetPositionAndRotation(hit.point + new Vector3(0, possible.transform.localScale.y * 1.5f, 0), Quaternion.identity);
            possible.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
        animalSpawnTime = UnityEngine.Random.Range(animalSpawnTimeMin, animalSpawnTimeMax);
        //Debug.Log(animalSpawnTime);
    }

    // Spawns a wolf somewhere at random
    private void SpawnWolf()
    {
        if (wolfPool.Count == 0) return;
        wolfSpawnTimer = 0;

        RaycastHit hit = FindValidSpawnPoint();
        
        GameObject possible = wolfPool.Pop();

        if (possible != null)
        {
            possible.SetActive(true);
            possible.transform.SetPositionAndRotation(hit.point + new Vector3(0, possible.transform.localScale.y * 1.5f, 0), Quaternion.identity);
            possible.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
        wolfSpawnTime = UnityEngine.Random.Range(wolfSpawnTimeMin, wolfSpawnTimeMax);
    }

    private void SpawnBigfoot()
    {
        if (bigfootAlive) return;
        bigfootSpawnTimer = 0;
        bigfootAlive = true;

        RaycastHit hit = FindValidSpawnPoint();

        activeBigfoot = Instantiate(bigfoot);
        activeBigfoot.transform.SetPositionAndRotation(hit.point + new Vector3(0, activeBigfoot.transform.localScale.y * 1.5f, 0), Quaternion.identity);
        bigfootSpawnTime *= bigfootRespawnModifier;
    }
    #endregion

    #region Object Pool Return
    public void RepoolAnimal(GameObject animal)
    {
        animal.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        animal.SetActive(false);
        animalPool.Push(animal);
    }

    public void RepoolWolf(GameObject wolf)
    {
        wolf.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        wolf.SetActive(false);
        wolfPool.Push(wolf);
    }

    public void DeadBigfoot()
    {
        bigfootAlive = false;
        Destroy(activeBigfoot);
    }

    public void TeleportBigfoot()
    {
        RaycastHit hit = FindValidSpawnPoint();
        activeBigfoot.transform.SetPositionAndRotation(hit.point + new Vector3(0, activeBigfoot.transform.localScale.y * 1.5f, 0), Quaternion.identity);
    }
    #endregion
}
