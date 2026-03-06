using System.Collections;
using UnityEngine;

/// <summary>
/// The base state machine class for all creatures in the game.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class CreatureStateMachine : MonoBehaviour
{
    #region Fields, Properties, etc.
    // Data values for this creature
    [Header("Data Values")]
    [SerializeField] protected float maxHealth;
    protected float health;
    [SerializeField] protected float sneakSpeed;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float runSpeed;
    [SerializeField] protected float distanceToDespawn;
    [SerializeField] protected float distanceToDetectPlayer;
    [SerializeField] protected float damage;
    [SerializeField] protected bool damageDealingActive;
    [SerializeField] protected float wanderTimeInterval;
    [SerializeField] protected float stalkingDistance;
    [SerializeField] protected float attackDistance;
    [SerializeField] protected float attackInterval;

    // References to the creature's subobjects
    [Header("Object References")]
    [SerializeField] private GameObject drop;
    [SerializeField] protected GameObject model;
    [SerializeField] protected GameObject hitbox;
    [SerializeField] protected AudioClip[] soundEffects;

    // Other object references
    protected GameObject player;
    protected State currentState;
    protected Rigidbody rb;
    protected AudioSource sfx;

    // Public getters for controller data
    public float WalkSpeed { get => moveSpeed; }
    public float RunSpeed { get => runSpeed; }
    public float StalkSpeed { get => sneakSpeed; }
    public float DespawnDistance { get => distanceToDespawn; }
    public float DetectPlayerDistance { get => distanceToDetectPlayer; }

    public float MaxWanderTime { get => wanderTimeInterval; }

    public Vector3 PlayerPosition { get => player.transform.position; }
    public Vector3 MyPosition { get => transform.position; }

    public float MaxStalkingDistance { get => stalkingDistance; }
    public float AttackDistance { get => attackDistance; }
    public float AttackInterval { get => attackInterval; }
    public bool Attacking { get; protected set; }

    // Data modifiable by states
    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float CurrentSpeed;
    [HideInInspector] public float TimeSinceAction;
    [HideInInspector] public float MovingTime;
    [HideInInspector] public float Aggression;

    #endregion

    #region Unity Methods
    protected void Awake()
    {
        health = maxHealth;
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
        sfx = GetComponent<AudioSource>();
    }

    private void Update()
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
        if (other.CompareTag("Player") && damageDealingActive)
        {
            player.GetComponent<PlayerController>().Hurt(damage);
        }
    }
    #endregion

    #region State Machine Methods
    /// <summary>
    /// Sets the active state for the state machine to a new one. <br></br>
    /// This calls <c>OnStateExit()</c> on the current state and <c>OnStateEnter()</c> on the new state.
    /// </summary>
    /// <param name="state">The new state to switch to.</param>
    public void SetState(State state)
    {
        currentState?.OnStateExit();
        currentState = state;
        currentState?.OnStateEnter();
    }

    /// <summary>
    /// Plays a random audio clip in the specified range.
    /// </summary>
    /// <param name="min">The minimum index for the audio clip to play.</param>
    /// <param name="max">The maximum index for the audio clip to play.</param>
    public virtual void PlayAudio(int min, int max)
    {
        sfx.clip = soundEffects[Random.Range(min, max)];
        sfx.Play();
    }

    /// <summary>
    /// Deals damage to the creature. Kills them if needed.
    /// </summary>
    /// <param name="damage">How much damage to deal.</param>
    public virtual void Hurt(int damage)
    {
        health -= damage;
        player.GetComponent<PlayerController>().OnHitSuccess();
        // todo: knockback function?

        if (health <= 0)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
            health = maxHealth;
            GameObject.Find("DataCollect").GetComponent<SaveData>().killCount++;
            RandomSpawner spawner = GameObject.Find("TerrainGenerator").GetComponent<RandomSpawner>();
            // todo: repooling but better/respawning but better
            System.Type type = GetType();

            if (type == typeof(BigfootStateController))
            {
                spawner.DeadBigfoot();
            }
            else
            {
                SetState(new DespawnState(this));
            }
        }
    }

    /// <summary>
    /// Moves this creature this frame. More specifically, adds the current speed and direction
    /// of this creature as a force, capped to its current speed.
    /// </summary>
    public virtual void Move()
    {
        rb.AddRelativeForce(CurrentSpeed * new Vector3(Direction.x, 0, Direction.z), ForceMode.Impulse);
        rb.linearVelocity = new Vector3(
            Mathf.Clamp(rb.linearVelocity.x, -CurrentSpeed, CurrentSpeed),
            rb.linearVelocity.y,
            Mathf.Clamp(rb.linearVelocity.z, -CurrentSpeed, CurrentSpeed)
        );
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Cancels this creature's movement.
    /// </summary>
    public virtual void DontMove()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        transform.rotation = Quaternion.identity;
        rb.angularVelocity.Set(0, 0, 0);
    }

    /// <summary>
    /// Rotates this creature's model to face the currently set direction.
    /// Note that the creature's root transform is not rotated, and all other move functions
    /// should not change that. It will fuck with most states.
    /// </summary>
    public virtual void FaceDirection()
    {
        model.transform.rotation = Quaternion.LookRotation(new Vector3(Direction.x, 0, Direction.z), Vector3.up);
        rb.angularVelocity.Set(0, 0, 0);
    }

    /// <summary>
    /// Performs the attack of this creature. This function defaults to the lunge
    /// attack behavior of Wolves.
    /// </summary>
    public virtual void Attack()
    {
        Vector3 lunge = new Vector3(Direction.x, 2, Direction.z) * CurrentSpeed;
        rb.AddRelativeForce(lunge, ForceMode.Impulse);
        StartCoroutine(HitboxCycle());
    }

    protected IEnumerator HitboxCycle()
    {
        hitbox.SetActive(true);
        yield return new WaitForSeconds(1f);
        hitbox.SetActive(false);
    }
    #endregion
}
