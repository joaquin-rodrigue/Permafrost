using System.Collections;
using UnityEngine;

/// <summary>
/// Main state machine for Bigfoot. He only shows up one at a time.
/// </summary>
public class BigfootStateController : CreatureStateMachine
{
    [Header("Bigfoot Data Values")]
    [SerializeField] private float behindPlayerAngleTolerance;
    [SerializeField] private float staringAngleTolerance;
    [SerializeField] private float maxStalkingDistance;
    [SerializeField] private float attackTimeLimit;
    [SerializeField] private float attackStateCooldown;
    [SerializeField] private float onHitAggroBoost;

    [Header("Light Related Data")]
    [SerializeField] private float lightCheckInterval;
    [SerializeField] private float lightAggroIncrease;

    public float LightCheckInterval { get => lightCheckInterval; }
    public float IdealStalkDistance { get => stalkingDistance; }
    public float MaxStalkDistance { get => maxStalkingDistance; }
    public float BehindPlayerDifference { get => behindPlayerAngleTolerance; }
    public float PlayerStaringDifference { get => 360 - staringAngleTolerance; }
    public float AttackStateCooldown { get =>  attackStateCooldown; }
    public float AggressionLimit { get => attackTimeLimit; }
    public bool IsHeRoaring { get; private set; }
    public Vector3 PlayerRotation { get => player.transform.rotation.eulerAngles; }
    public Vector3 ModelRotation { get => model.transform.rotation.eulerAngles; }

    // Data modifiable by states
    [HideInInspector] public Transform TargetLightSource;
    [HideInInspector] public float TimeSinceLightCheck;
    [HideInInspector] public float TimeSinceAttackLeave;

    #region Unity Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected new void Awake()
    {
        base.Awake();
        SetState(new BigfootWanderState(this));
    }
    #endregion

    #region State Machine Methods
    public override void Hurt(int damage)
    {
        base.Hurt(damage);
        TimeSinceAttackLeave += onHitAggroBoost;
        if (TimeSinceAttackLeave > attackStateCooldown) 
        {
            SetState(new BigfootAttackState(this));
        }
    }

    // TODO: finish attack
    public override void Attack()
    {
        Debug.Log("attacking");
        StartCoroutine(BigfootAttackAnim());
    }

    private IEnumerator BigfootAttackAnim()
    {
        anim.SetBool("attacking", true);
        PlayAudio(5, 8);
        Attacking = true;
        yield return new WaitForSeconds(0.25f);

        StartCoroutine(HitboxCycle());
        yield return new WaitForSeconds(0.7f);
        Attacking = false;
        anim.SetBool("attacking", false);
    }
    #endregion

    public void HeRoar()
    {
        StartCoroutine(Roar());
    }

    private IEnumerator Roar()
    {
        anim.SetBool("roar", true);
        PlayAudio(9, 11);
        IsHeRoaring = true;
        yield return new WaitForSeconds(2.45f);
        anim.SetBool("roar", false);
        IsHeRoaring = false;
    }
}
