using UnityEngine;

public class ItemLibrary : MonoBehaviour
{
    [SerializeField] private GameObject[] physicalItems;
    [SerializeField] private string[] physicalItemNames;

    public void CreatePhysicalItem(string itemName, Transform position)
    {
        int index = -1;
        for (int i = 0; i < physicalItemNames.Length; i++)
        {
            if (itemName == physicalItemNames[i])
            {
                index = i;
                break;
            }
        }
        if (index == -1)
        {
            Debug.LogWarning("Tried creating an item that doesn't exist!");
            return;
        }
        GameObject theItem = Instantiate(physicalItems[index], position.position + position.forward, Quaternion.identity);
    }
}
