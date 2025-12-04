using UnityEngine;

/// <summary>
/// Main state machine for animals that spawn on the map.
/// </summary>
public class AnimalStateController : MonoBehaviour
{
    // Data values set in the editor, mostly readonly once the enemy is spawned
    [Header("Data Values")]
    [SerializeField] private float maxHealth = 20;
    private float health;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float distanceToRunFromPlayer;
    [SerializeField] private float distanceToDespawn;
    public float wanderTimeInterval;

    [Header("Object References")]
    [SerializeField] private GameObject drop;
    [SerializeField] private GameObject model;

    // Public getters for controller data
    public float WalkSpeed { get => moveSpeed; }
    public float RunSpeed { get => runSpeed; } 
    public float MinRunDistance { get => distanceToRunFromPlayer; }
    public Vector3 PlayerPosition { get => player.transform.position; }
    public float DespawnDistance { get => distanceToDespawn; }

    // Other object refs
    private GameObject player;
    private AnimalState currentState;
    private Rigidbody rb;

    // Data modifiable by states
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float movingTime;
    [HideInInspector] public float timeSinceWanderCheck;

    #region Unity Methods
    void Awake()
    {
        health = maxHealth;
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
        SetState(new AnimalWander(this));
    }

    void Update()
    {
        currentState.CheckTransitions();
        currentState.Act();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerWeapon"))
        {
            Hurt(player.GetComponent<PlayerController>().CurrentDamage);
        }
    }
    #endregion

    #region State Machine Methods
    /// <summary>
    /// Sets the active state for the state machine to a new one. <br></br>
    /// This calls <c>OnStateExit()</c> on the current state and <c>OnStateEnter()</c> on the new state.
    /// </summary>
    /// <param name="state">The new state to switch to.</param>
    public void SetState(AnimalState state)
    {
        currentState?.OnStateExit();
        currentState = state;
        currentState?.OnStateEnter();
    }

    /// <summary>
    /// Deals damage to the animal. Kills them if needed.
    /// </summary>
    /// <param name="damage">How much damage to deal.</param>
    public void Hurt(int damage)
    {
        health -= damage;
        player.GetComponent<PlayerController>().MeleeHit(); 

        if (health <= 0)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
            health = maxHealth;
            GameObject.Find("DataCollect").GetComponent<SaveData>().killCount++;
            GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>().RepoolAnimal(gameObject);
        }
    }

    /// <summary>
    /// Tells this object to move this frame.
    /// </summary>
    public void Move()
    {
        //Vector2 targetVelocity = Vector3.forward * moveSpeed;
        //transform.Translate(targetVelocity);
        //rb.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.y);
        //Debug.Log(direction);
        rb.AddRelativeForce(currentSpeed * new Vector3(direction.x, 0, direction.y), ForceMode.Impulse);
        rb.linearVelocity = new Vector3(
            Mathf.Clamp(rb.linearVelocity.x, -currentSpeed, currentSpeed),
            rb.linearVelocity.y,
            Mathf.Clamp(rb.linearVelocity.z, -currentSpeed, currentSpeed)
        );
        transform.rotation = Quaternion.identity;
    }

    public void DontMove()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        transform.rotation = Quaternion.identity;
        rb.angularVelocity.Set(0, 0, 0);
    }

    public void FaceDirection()
    {
        model.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up); //Quaternion.Euler(0, (direction.y > 0 ? 0 : 180) + (90 + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), 0);
        // I guess I just have no clue how rotations work, tried doing this with Euler but failed miserably
        rb.angularVelocity.Set(0, 0, 0);
        //Debug.Log(direction);
        //Debug.Log(model.transform.rotation.eulerAngles.y);
        //Debug.Log(transform.rotation.eulerAngles.y);
    }
    #endregion
}
