using UnityEngine;

/// <summary>
/// Controller for animal movements. Just in general.
/// </summary>
public class AnimalMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToRunFromPlayer;
    [SerializeField] private GameObject drop;

    private Vector2 direction;
    private float movingTime;
    private Rigidbody rb;
    private float health = 20;

    // Just getting some references/setting an interval for calling rng
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        InvokeRepeating(nameof(MoveRandomly), 0.5f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        // Find distance to player
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= distanceToRunFromPlayer)
        {
            // run from the player
            Vector3 runDirection = moveSpeed * (new Vector3(direction.x, 0, direction.y) + (transform.position - player.transform.position).normalized); // vector away with random variation
            rb.linearVelocity = transform.rotation * new Vector3(runDirection.x, rb.linearVelocity.y, runDirection.z);
            return;
        }

        if (movingTime <= 0)
        {
            return;
        }

        // Moving
        Vector2 targetVelocity = direction * moveSpeed;
        rb.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);

        movingTime -= Time.deltaTime;
    }

    // Decides a nenw random movement direction and mmoving time
    private void MoveRandomly()
    {
        direction = new Vector2(Random.value - 0.5f, Random.value - 0.5f).normalized;
        movingTime = Random.value * 4;
    }

    // Get hit
    public void Hurt(int damage)
    {
        health -= damage;
        Debug.Log(health);

        if (health <= 0)
        {
            // do something
            Instantiate(drop, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            Hurt(5);
        }
    }
}
