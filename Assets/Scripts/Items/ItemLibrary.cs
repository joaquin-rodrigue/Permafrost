using UnityEngine;

/// <summary>
/// Used for looking up different item names, models, etc.
/// todo: singletonize
/// </summary>
public class ItemLibrary : MonoBehaviour
{
    [Header("In-World Items")]
    [SerializeField] private GameObject[] physicalItems;
    [SerializeField] private string[] ItemNames;

    [Header("Item View Model Data")]
    [SerializeField] private Mesh[] itemModels;
    [SerializeField] private Material[] itemMaterials;
    [SerializeField] private Vector3[] itemViewModelScales;
    [SerializeField] private Quaternion[] itemViewModelRotations;

    [Header("Inventory Item Renders")]
    [SerializeField] private Sprite[] itemRenders;

    /// <summary>
    /// Searches the item list and returns the index of the associated item.
    /// Utilizes the set of item names to determine what index an item is at.
    /// </summary>
    /// <param name="itemName">The name of the item.</param>
    /// <returns>The index in the item list(s) that the item is at, or 01 if not found.</returns>
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

    /// <summary>
    /// Spawns in an item prefab in-world at the given transform's position. The item will not be instantiated as a child.
    /// Will not spawn the item if the given item name does not correspond to an actual item.
    /// </summary>
    /// <param name="itemName">The name of the item to spawn.</param>
    /// <param name="position">The transform at whose position the item will be spawned.</param>
    public void CreatePhysicalItem(string itemName, Transform position)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning($"Tried creating an item '{itemName}' that doesn't exist!");
            return;
        }
        GameObject theItem = Instantiate(physicalItems[index], position.position + position.forward, Quaternion.identity);
    }

    /// <summary>
    /// Retrieves the given item's model, if found.
    /// </summary>
    /// <param name="itemName">The name of the item to find the model for.</param>
    /// <returns>The Mesh corresponding to the item, or null if the item doesn't exist.</returns>
    public Mesh GetItemModel(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning($"Tried getting mesh for item '{itemName}' that doesn't exist!");
            return null;
        }
        return itemModels[index];
    }

    /// <summary>
    /// Retrieves the given item's material, if found.
    /// </summary>
    /// <param name="itemName">The name of the item to find the material for.</param>
    /// <returns>The Material corresponding to the item, or null if the item doesn't exist.</returns>
    public Material GetItemMaterial(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning($"Tried getting material for item '{itemName}' that doesn't exist!");
            return null;
        }
        return itemMaterials[index];
    }

    /// <summary>
    /// Gets the view model scale for the given item, if found.
    /// </summary>
    /// <param name="itemName">The name of the item to find a view model scale for.</param>
    /// <returns>A Vector3 of the scale of the item's view model, or <c>Vector3.one</c> if the item isn't found.</returns>
    public Vector3 GetItemScale(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning($"Tried getting scale for item '{itemName}' that doesn't exist!");
            return Vector3.one;
        }
        return itemViewModelScales[index];
    }

    /// <summary>
    /// Gets the view model rotation for the given item, if found.
    /// </summary>
    /// <param name="itemName">The name of the item to find a view model rotation for.</param>
    /// <returns>A Quaternion for the item's view model rotation, or <c>Quaternion.identity</c> if the item isn't found.</returns>
    public Quaternion GetItemRotation(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning($"Tried getting rotation for item '{itemName}' that doesn't exist!");
            return Quaternion.identity;
        }
        return itemViewModelRotations[index];
    }

    /// <summary>
    /// Gets the inventory render for the given item, if found.
    /// </summary>
    /// <param name="itemName">The name of the item to find an inventory sprite for.</param>
    /// <returns>The inventory sprite for the item, or null if the item isn't found.</returns>
    public Sprite GetItemInventoryRender(string itemName)
    {
        int index = SearchItemList(itemName);
        if (index == -1)
        {
            Debug.LogWarning($"Tried getting inventory sprite for item '{itemName}' that doesn't exist!");
            return null;
        }
        return itemRenders[index];
    }
}
