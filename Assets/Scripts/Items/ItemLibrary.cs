using UnityEngine;

public class ItemLibrary : MonoBehaviour
{
    [SerializeField] private GameObject[] physicalItems;
    [SerializeField] private string[] ItemNames;

    [SerializeField] private Mesh[] itemModels;
    [SerializeField] private Material[] itemMaterials;

    private int SearchItemList(string itemName)
    {
        if (itemName == null) return -1;
        for (int i = 0; i < ItemNames.Length; i++)
        {
            if (itemName == ItemNames[i])
            {
                return i;
            }
        }
        return -1;
    }

    public void CreatePhysicalItem(string itemName, Transform position)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning("Tried creating an item that doesn't exist!");
            return;
        }
        GameObject theItem = Instantiate(physicalItems[index], position.position + position.forward, Quaternion.identity);
    }

    public Mesh GetItemModel(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning("Tried getting mesh for item that doesn't exist!");
            return null;
        }
        // this feels scary
        Mesh mesh = itemModels[index];
        return mesh;
    }

    public Material GetItemMaterial(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning("Tried getting material for item that doesn't exist!");
            return null;
        }
        Material mat = itemMaterials[index];
        return mat;
    }
}
