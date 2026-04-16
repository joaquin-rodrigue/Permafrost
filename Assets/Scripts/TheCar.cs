using UnityEngine;

public class TheCar : MonoBehaviour
{
    private int fuelAdded;

    public void AddAFuel()
    {
        fuelAdded++;
    }

    public int FuelCount()
    {
        return fuelAdded;
    }

    /// <summary>
    /// bandaid fix please 
    /// </summary>
    private void Update()
    {
        if (transform.position.y < 0) 
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            transform.position = new Vector3(128, 150, 135);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
