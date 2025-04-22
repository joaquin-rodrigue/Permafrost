using UnityEngine;

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
    [SerializeField] private int treeCountMax;
    [SerializeField] private int treeCountMin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        treeCount = Random.Range(treeCountMin, treeCountMax);
        bool foundValidSpawn = false;
        RaycastHit hit = new();

        for (int i = 0; i < treeCount;)
        {
            foundValidSpawn = false;
            while (!foundValidSpawn)
            {
                foundValidSpawn = Physics.Raycast(new Vector3(Random.Range(100, 900), 80, Random.Range(100, 900)), Vector3.down, out hit, 80);
            }

            int countInArea = Random.Range(1, 10);

            for (int j = 0; j < countInArea; j++)
            {
                RaycastHit individual = new();
                Physics.Raycast(new Vector3(hit.point.x + Random.Range(-10, 10), hit.point.y + 20, hit.point.z + Random.Range(-10, 10)), Vector3.down, out individual, 50);
                Instantiate(tree, individual.point, Quaternion.identity);
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
            SpawnAnimal();
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
            foundValidSpawn = Physics.Raycast(new Vector3(Random.Range(100, 900), 80, Random.Range(100, 900)), Vector3.down, out hit, 80);
        }

        GameObject newAnimal = Instantiate(animal, hit.point + Vector3.up, Quaternion.identity);
        animalSpawnTime = Random.Range(animalSpawnTimeMin, animalSpawnTimeMax);
    }
}
