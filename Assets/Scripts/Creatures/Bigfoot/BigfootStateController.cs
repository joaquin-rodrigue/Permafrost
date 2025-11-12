using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

/// <summary>
/// Main state machine for Bigfoot. He only shows up one at a time.
/// </summary>
public class BigfootStateController : MonoBehaviour
{
    [Header("Data Values")]
    [SerializeField] private float maxHealth = 200;
    private float health;
    [SerializeField] private float damage;
    [SerializeField] private float stalkSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float distanceToDetectPlayer;
    [SerializeField] private float behindPlayerAngleTolerance;
    [SerializeField] private float staringAngleTolerance;
    [SerializeField] private float stalkingDistance;
    [SerializeField] private float maxStalkingDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private float wanderTimeInterval;
    [SerializeField] private float attackTimeInterval;
    [SerializeField] private float attackTimeLimit;
    [SerializeField] private float attackStateCooldown;
    [SerializeField] private float onHitAggroBoost;

    [Header("Light Related Data")]
    [SerializeField] private float lightCheckInterval;
    [SerializeField] private float lightAggroIncrease;

    [Header("Object References")]
    [SerializeField] private GameObject drop;
    [SerializeField] private GameObject model;
    public float StalkSpeed { get => stalkSpeed; }
    public float MoveSpeed { get => walkSpeed; }
    public float RunSpeed { get => runSpeed; }
    public float DetectPlayerDistance { get => distanceToDetectPlayer; }
    public float MaxWanderTime { get => wanderTimeInterval; }
    public float LightCheckInterval { get => lightCheckInterval; }
    public float AttackInterval { get => attackTimeInterval; }
    public float AttackDistance { get => attackDistance; }
    public float IdealStalkDistance { get => stalkingDistance; }
    public float MaxStalkDistance { get => maxStalkingDistance; }
    public float BehindPlayerDifference { get => behindPlayerAngleTolerance; }
    public float PlayerStaringDifference { get => 360 - staringAngleTolerance; }
    public float AttackStateCooldown { get =>  attackStateCooldown; }
    public float AggressionLimit { get => attackTimeLimit; }
    public Vector3 PlayerPosition { get => player.transform.position; }
    public Vector3 PlayerRotation { get => player.transform.rotation.eulerAngles; }
    public Vector3 ModelRotation { get => model.transform.rotation.eulerAngles; }
    public bool Attacking { get; private set; }

    // Other object refs
    private BigfootState currentState;
    private GameObject player;
    private Rigidbody rb;

    // Data modifiable by states
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public Transform TargetLightSource;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float movingTime;
    [HideInInspector] public float timeSinceWanderCheck;
    [HideInInspector] public float aggression;
    [HideInInspector] public float timeSinceLightCheck;
    [HideInInspector] public float timeSinceAttackLeave;

    #region Unity Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        health = maxHealth;
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
        SetState(new BigfootWander(this));
    }

    // Update is called once per frame
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
    public void SetState(BigfootState state)
    {
        currentState?.OnStateExit();
        currentState = state;
        currentState?.OnStateEnter();
    }

    public void Hurt(int damage)
    {
        health -= damage;
        rb.AddRelativeForce(player.transform.position - transform.position, ForceMode.Impulse);
        player.GetComponent<PlayerController>().MeleeHit();
        timeSinceAttackLeave += onHitAggroBoost;
        if (timeSinceAttackLeave > attackStateCooldown) 
        {
            SetState(new BigfootAttack(this));
        }

        if (health <= 0)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
            health = maxHealth;
            //GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>()
        }
    }

    public void Move()
    {
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

    public void Attack()
    {
        Debug.Log("attacking");

        
    }

    public void FaceDirection()
    {
        model.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up); //Quaternion.Euler(0, (direction.y > 0 ? 0 : 180) + (90 + Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), 0);
        // I guess I just have no clue how rotations work, tried doing this with Euler but failed miserably
        rb.angularVelocity.Set(0, 0, 0);
        Debug.Log(direction);
        //Debug.Log(model.transform.rotation.eulerAngles.y);
        //Debug.Log(transform.rotation.eulerAngles.y);
    }
    #endregion
}
