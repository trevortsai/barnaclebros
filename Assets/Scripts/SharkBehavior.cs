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
    public float attackRange            = 4f;
    public float crabDetectRange        = 8f;
    public float retreatArriveThreshold = 1.5f;
    public float retreatDuration        = 6f;

    private SharkState   _state = SharkState.Swimming;
    private SharkAnimator _anim;
    private CrabBehavior  _crab;
    private float         _retreatTimer;

    // ── lifecycle ────────────────────────────────────────────────────────
    void Start()
    {
        _anim = GetComponent<SharkAnimator>();
        _crab = FindFirstObjectByType<CrabBehavior>();
        _anim.Loop(SharkAnimator.SWIM);
    }

    void Update()
    {
        switch (_state)
        {
            case SharkState.Swimming:         TickSwimming();   break;
            case SharkState.Attacking:        TickAttacking();  break;
            case SharkState.Hit:              TickHit();        break;
            case SharkState.Retreating:       TickRetreating(); break;
            case SharkState.WaitingAtRetreat: TickWaiting();    break;
        }
    }

    // ── state ticks ──────────────────────────────────────────────────────
    void TickSwimming()
    {
        if (player == null) return;

        // Crab nearby → take hit, flee
        if (_crab != null &&
            Vector3.Distance(transform.position, _crab.transform.position) < crabDetectRange)
        {
            _crab.TriggerAttack();
            _state = SharkState.Hit;
            _anim.Play(SharkAnimator.HIT);
            return;
        }

        // Player in range → bite
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            _state = SharkState.Attacking;
            _anim.Play(SharkAnimator.BITE);
            return;
        }

        MoveToward(player.position, followSpeed);
        _anim.Loop(SharkAnimator.SWIM);
    }

    void TickAttacking()
    {
        if (_anim.IsFinished(SharkAnimator.BITE))
        {
            _state = SharkState.Swimming;
            _anim.Loop(SharkAnimator.SWIM);
        }
    }

    void TickHit()
    {
        if (_anim.IsFinished(SharkAnimator.HIT))
        {
            _state = SharkState.Retreating;
            _anim.Loop(SharkAnimator.SWIM);
        }
    }

    void TickRetreating()
    {
        if (retreatPoint == null) return;
        MoveToward(retreatPoint.position, retreatSpeed);
        _anim.Loop(SharkAnimator.SWIM);

        if (Vector3.Distance(transform.position, retreatPoint.position) < retreatArriveThreshold)
        {
            _retreatTimer = retreatDuration;
            _state = SharkState.WaitingAtRetreat;
            _anim.Play(SharkAnimator.IDLE);
        }
    }

    void TickWaiting()
    {
        _retreatTimer -= Time.deltaTime;
        if (_retreatTimer <= 0f)
        {
            _crab?.StopAttack();
            _state = SharkState.Swimming;
            _anim.Loop(SharkAnimator.SWIM);
        }
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
