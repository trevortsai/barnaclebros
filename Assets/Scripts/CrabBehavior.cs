using UnityEngine;

public class CrabBehavior : MonoBehaviour
{
    [Header("Wandering")]
    public float walkSpeed = 1.2f;
    public float wanderRadius = 12f;
    public float idleTimeAtWaypoint = 2f;

    [Header("Attack")]
    public float attackDuration = 3f;

    private Animator _animator;
    private Vector3 _homePosition;
    private Vector3 _waypoint;
    private float _idleTimer;
    private bool _isIdle;
    private bool _isAttacking;
    private float _attackTimer;

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _homePosition = transform.position;
        PickWaypoint();
    }

    void Update()
    {
        if (_isAttacking)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
                EndAttack();
            return;
        }

        if (_isIdle)
        {
            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f)
            {
                _isIdle = false;
                PickWaypoint();
                _animator?.Play("Walk");
            }
            return;
        }

        WalkToWaypoint();
    }

    void WalkToWaypoint()
    {
        Vector3 toWaypoint = _waypoint - transform.position;
        toWaypoint.y = 0f;

        if (toWaypoint.magnitude < 0.4f)
        {
            _isIdle = true;
            _idleTimer = idleTimeAtWaypoint;
            _animator?.Play("Idle");
            return;
        }

        Vector3 dir = toWaypoint.normalized;
        transform.position += dir * walkSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
    }

    void PickWaypoint()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        _waypoint = _homePosition + new Vector3(rand.x, 0f, rand.y);
    }

    public void TriggerAttack()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        _attackTimer = attackDuration;
        _animator?.Play("Attack");
    }

    public void StopAttack()
    {
        EndAttack();
    }

    void EndAttack()
    {
        _isAttacking = false;
        _isIdle = false;
        PickWaypoint();
        _animator?.Play("Walk");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? _homePosition : transform.position, wanderRadius);
    }
}
