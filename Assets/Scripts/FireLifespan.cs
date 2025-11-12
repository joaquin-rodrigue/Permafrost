using UnityEngine;

/// <summary>
/// Simple script to keep track of a campfire's timer before it burns out.
/// </summary>
public class FireLifespan : MonoBehaviour
{
    private float lifespan;

    void Update()
    {
        lifespan -= Time.deltaTime;
        if (lifespan < 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adds fuel to the fire and keeps it around longer.
    /// </summary>
    /// <param name="fuel">The time, in seconds, to add.</param>
    public void AddFuel(float fuel)
    {
        lifespan += fuel;
    }
}
