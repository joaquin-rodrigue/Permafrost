using UnityEngine;

/// <summary>
/// Generates objects in the world randomly.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject animal;
    [SerializeField] private GameObject tree;

    private float animalSpawnTime;
    private float animalSpawnTimer;
    private int animalCount;
    [SerializeField] private float animalSpawnTimeMax;
    [SerializeField] private float animalSpawnTimeMin;

    private int treeCount;
    private int maxLoopCountDuringGeneration = 1000;
    [SerializeField] private int treeCountMax;
    [SerializeField] private int treeCountMin;

    [SerializeField] private RandomTerrainGenerator generation;

    // Runs the code to spawn one-time objects
    public void Generate()
    {
        treeCount = generation.random.Next(treeCountMin, treeCountMax);
        bool foundValidSpawn = false;
        RaycastHit hit = new();

        int loopCount = 0; // prevent rng from causing an infinite loop
        for (int i = 0; i < treeCount && loopCount < maxLoopCountDuringGeneration;)
        {
            foundValidSpawn = false;
            while (!foundValidSpawn && loopCount < maxLoopCountDuringGeneration)
            {
                foundValidSpawn = Physics.Raycast(new Vector3(
                    generation.random.Next((int) generation.MapBounds.min.x, (int) generation.MapBounds.max.x), 
                    120, 
                    generation.random.Next((int)generation.MapBounds.min.z, (int)generation.MapBounds.max.z)), 
                    Vector3.down, 
                    out hit, 
                    120
                    //LayerMask.NameToLayer("Terrain")
                );
                loopCount++;
            }
            //Debug.Log(hit.point);

            if (loopCount > maxLoopCountDuringGeneration)
            {
                Debug.Log("no spawns left!");
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
                    50
                //LayerMask.NameToLayer("Terrain")
                ) && loopCount < maxLoopCountDuringGeneration) loopCount++;
                Instantiate(tree, chosenPoint.point, Quaternion.identity);
                i++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        animalSpawnTimer += Time.deltaTime;
        if (animalSpawnTimer > animalSpawnTime)
        {
            //SpawnAnimal();
        }
    }

    // Spawns an animal somewhere at random
    private void SpawnAnimal()
    {
        animalSpawnTimer = 0;
        bool foundValidSpawn = false;
        RaycastHit hit = new();
        while (!foundValidSpawn)
        {
            foundValidSpawn = Physics.Raycast(
                new Vector3(
                    Random.Range((int)generation.MapBounds.min.x, (int)generation.MapBounds.max.x), 
                    80, 
                    Random.Range((int)generation.MapBounds.min.x, (int)generation.MapBounds.max.x)
                ), 
                Vector3.down, 
                out hit, 
                80, 
                LayerMask.NameToLayer("Terrain")
            );
        }

        GameObject newAnimal = Instantiate(animal, hit.point + Vector3.up, Quaternion.identity);
        animalSpawnTime = Random.Range(animalSpawnTimeMin, animalSpawnTimeMax);
    }
}
