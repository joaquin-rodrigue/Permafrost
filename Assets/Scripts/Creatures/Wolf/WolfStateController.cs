using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Main state machine for wolves that spawn on the map.
/// </summary>
public class WolfStateController : MonoBehaviour
{
    // Data values set in the editor, mostly readonly once the enemy is spawned
    [Header("Data Values")]
    [SerializeField] private float maxHealth = 35;
    private float health;
    [SerializeField] private float damage;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float stalkSpeed;
    [SerializeField] private float distanceToDetectPlayer;
    [SerializeField] private float stalkingDistance;
    [SerializeField] private float wanderTimeInterval;
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackTimeInterval;
    [SerializeField] private float distanceToDespawn;

    [Header("Object References")]
    [SerializeField] private GameObject drop;
    [SerializeField] private GameObject model;

    // Public getters for controller data
    public float MoveSpeed { get => moveSpeed; }
    public float RunSpeed { get => runSpeed; }
    public float StalkSpeed { get => stalkSpeed; }
    public float DetectPlayerDistance { get => distanceToDetectPlayer; }
    public Vector3 PlayerPosition { get => player.transform.position; }
    public float MaxWanderTime { get => wanderTimeInterval; }
    public float MaxStalkDistance { get =>  stalkingDistance; }
    public float AttackDistance { get => attackDistance; }
    public float AttackInterval { get => attackTimeInterval; }
    public float DespawnDistance { get => distanceToDespawn; }

    // Other object refs
    private WolfState currentState;
    private GameObject player;
    private Rigidbody rb;

    // Data modifiable by states
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float movingTime;
    [HideInInspector] public float timeSinceWanderCheck;
    [HideInInspector] public float aggression;

    #region Unity Methods
    void Awake()
    {
        health = maxHealth;
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
        Debug.Log("wolf spawned at " + transform.position + " while player at " + player.transform.position);
        SetState(new WolfWander(this));
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
        if (other.CompareTag("Player"))
        {
            player.GetComponent<PlayerController>().Hurt(damage);
        }
    }
    #endregion

    #region State Machine Methods
    /// <summary>
    /// Sets the active state for the state machine to a new one.<br></br>
    /// This calls <c>OnStateExit()</c> on the current state and <c>OnStateEnter()</c> on the new state.
    /// </summary>
    /// <param name="state">The new state to switch to.</param>
    public void SetState(WolfState state)
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
        rb.AddRelativeForce(player.transform.position - transform.position, ForceMode.Impulse);
        player.GetComponent<PlayerController>().MeleeHit();

        if (health <= 0)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
            GameObject.Find("DataCollect").GetComponent<SaveData>().killCount++;
            health = maxHealth;
            GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>().RepoolWolf(gameObject);
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
        rb.AddRelativeForce(currentSpeed * new Vector3(direction.x, 0, direction.z), ForceMode.Impulse);
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
    }

    /// <summary>
    /// Tells the wolf to attack on this frame.
    /// </summary>
    public void Attack()
    {
        Debug.Log("attacking");

        Vector3 lunge = new Vector3(direction.x * 2, 2, direction.y * 2) * runSpeed;
        rb.AddRelativeForce(lunge, ForceMode.Impulse);
    }

    public void FaceDirection()
    {
        model.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up); //Quaternion.Euler(0, (direction.y > 0 ? 0 : 180) + (90 + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), 0);
        // I guess I just have no clue how rotations work, tried doing this with Euler but failed miserably
        rb.angularVelocity.Set(0, 0, 0);
        Debug.Log(direction);
        //Debug.Log(model.transform.rotation.eulerAngles.y);
        //Debug.Log(transform.rotation.eulerAngles.y);
    }
    #endregion
}
