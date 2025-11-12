using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickupable : MonoBehaviour
{
    [SerializeField] private ItemAttributes item;
    public ItemAttributes Item { get { return item; } }
}
