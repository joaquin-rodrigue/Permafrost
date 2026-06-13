using UnityEngine;

using Permafrost.Items;

[RequireComponent(typeof(Collider))]
public class Pickupable : MonoBehaviour
{
    [SerializeField] private ItemAttributes item;
    private Collider pickupTrigger;
    private float pickupLockout;
    public bool Collected { get; private set; } = false;

    private void Awake()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider.isTrigger)
            {
                pickupTrigger = collider;
                break;
            }
        }
        pickupTrigger.enabled = false;
        pickupLockout = 1.5f;
    }

    public void Collect()
    {
        Collected = true;
    }

    private void Update()
    {
        pickupLockout -= Time.deltaTime;
        if (pickupLockout < 0)
        {
            pickupTrigger.enabled = true;
        }
    }

    public ItemAttributes Item { get { return item; } }
}
