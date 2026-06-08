using UnityEngine;

public class SharkBehavior : MonoBehaviour
{
    public enum SharkState { Swimming, Attacking, Hit, Retreating, WaitingAtRetreat }

    [Header("References")]
    public Transform player;
    public Transform retreatPoint;

    [Header("Movement")]
    public float followSpeed = 3f;
    public float retreatSpeed = 6f;
    public Vector3 rotationOffset = new Vector3(0f, 90f, 0f);

    [Header("Behaviour")]
    public float attackRange     = 4f;   // distance to player that triggers Bite
    public float crabDetectRange = 8f;   // distance to crab that triggers Hit + retreat
    public float retreatArriveThreshold = 1.5f;
    public float retreatDuration = 6f;

    // ── private state ────────────────────────────────────────────────────
    private SharkState _state = SharkState.Swimming;
    private Animator   _animator;
    private CrabBehavior _crab;
    private float _retreatTimer;
    private string _currentAnim = "";

    // Animation clip names (must match states in the Animator Controller)
    private const string ANIM_SWIM   = "Swim Horizontal";
    private const string ANIM_BITE   = "Bite";
    private const string ANIM_HIT    = "Hit";
    private const string ANIM_IDLE   = "Idle";

    // ── lifecycle ────────────────────────────────────────────────────────
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _crab     = FindFirstObjectByType<CrabBehavior>();
        PlayAnim(ANIM_SWIM);
    }

    void Update()
    {
        switch (_state)
        {
            case SharkState.Swimming:
                TickSwimming();
                break;
            case SharkState.Attacking:
                TickAttacking();
                break;
            case SharkState.Hit:
                TickHit();
                break;
            case SharkState.Retreating:
                TickRetreating();
                break;
            case SharkState.WaitingAtRetreat:
                TickWaiting();
                break;
        }
    }

    // ── state ticks ──────────────────────────────────────────────────────
    void TickSwimming()
    {
        if (player == null) return;

        // Crab nearby → get hit, flee
        if (_crab != null && Vector3.Distance(transform.position, _crab.transform.position) < crabDetectRange)
        {
            _crab.TriggerAttack();
            EnterHit();
            return;
        }

        // Player in attack range → bite
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            EnterAttack();
            return;
        }

        MoveToward(player.position, followSpeed);
        LoopAnim(ANIM_SWIM);
    }

    void TickAttacking()
    {
        // Wait for Bite clip to finish, then return to swimming
        if (AnimFinished(ANIM_BITE))
        {
            _state = SharkState.Swimming;
            PlayAnim(ANIM_SWIM);
        }
    }

    void TickHit()
    {
        // Wait for Hit clip to finish, then retreat
        if (AnimFinished(ANIM_HIT))
        {
            _state = SharkState.Retreating;
            PlayAnim(ANIM_SWIM);
        }
    }

    void TickRetreating()
    {
        if (retreatPoint == null) return;
        MoveToward(retreatPoint.position, retreatSpeed);
        LoopAnim(ANIM_SWIM);

        if (Vector3.Distance(transform.position, retreatPoint.position) < retreatArriveThreshold)
        {
            _retreatTimer = retreatDuration;
            _state = SharkState.WaitingAtRetreat;
            PlayAnim(ANIM_IDLE);
        }
    }

    void TickWaiting()
    {
        _retreatTimer -= Time.deltaTime;
        if (_retreatTimer <= 0f)
        {
            _crab?.StopAttack();
            _state = SharkState.Swimming;
            PlayAnim(ANIM_SWIM);
        }
    }

    // ── state transitions ────────────────────────────────────────────────
    void EnterAttack()
    {
        _state = SharkState.Attacking;
        PlayAnim(ANIM_BITE);
    }

    void EnterHit()
    {
        _state = SharkState.Hit;
        PlayAnim(ANIM_HIT);
    }

    // ── animation helpers ────────────────────────────────────────────────

    // Play a state only when it differs from the currently playing one
    void PlayAnim(string animName)
    {
        if (_currentAnim == animName) return;
        _currentAnim = animName;
        _animator?.Play(animName, 0, 0f);
    }

    // Restart an animation when it reaches the end (manual looping)
    void LoopAnim(string animName)
    {
        PlayAnim(animName);
        if (_animator == null) return;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(animName) && info.normalizedTime >= 1f)
            _animator.Play(animName, 0, 0f);
    }

    // Returns true once the named clip has played through
    bool AnimFinished(string animName)
    {
        if (_animator == null) return true;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animName) && info.normalizedTime >= 1f;
    }

    // ── movement ─────────────────────────────────────────────────────────
    void MoveToward(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            look * Quaternion.Euler(rotationOffset),
            5f * Time.deltaTime
        );
    }

    // ── gizmos ───────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, crabDetectRange);
    }
}
