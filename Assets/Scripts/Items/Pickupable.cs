using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickupable : MonoBehaviour
{
    [SerializeField] private ItemAttributes item;
    private Collider pickupTrigger;
    private float pickupLockout;

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
