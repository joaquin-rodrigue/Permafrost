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
}
