using UnityEngine;

public class FireLifespan : MonoBehaviour
{
    private float lifespan;

    // Update is called once per frame
    void Update()
    {
        lifespan -= Time.deltaTime;
        if (lifespan < 0)
        {
            Destroy(gameObject);
        }
    }

    public void AddFuel(float fuel)
    {
        lifespan += fuel;
    }
}
