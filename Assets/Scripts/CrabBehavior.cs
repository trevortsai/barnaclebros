using UnityEngine;

/// <summary>
/// Physics-based crab AI.
/// The crab has a Rigidbody (gravity on, rotation X/Z frozen) and a BoxCollider.
/// Horizontal movement is applied via Rigidbody.linearVelocity in FixedUpdate so
/// gravity keeps the crab pressed against the sea floor at all times.
/// Waypoints are projected onto the terrain with a downward raycast so the crab
/// follows the contour of the ocean floor rather than walking at a fixed Y.
///
/// Ground snap: every FixedUpdate a downward ray (cast from high above to avoid
/// self-collision) finds the terrain surface and forces the pivot to sit on it.
/// groundSnapOffset = 0 means feet flush with terrain; raise it slightly if the
/// visual mesh sinks in; lower it (negative) if it still floats.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CrabBehavior : MonoBehaviour
{
    [Header("Wandering")]
    public float walkSpeed          = 1.2f;
    public float wanderRadius       = 12f;
    public float idleTimeAtWaypoint = 2f;
    [Tooltip("Horizontal distance at which the crab counts a waypoint as reached.")]
    public float arrivalDistance    = 0.5f;

    [Header("Attack")]
    public float attackDuration = 3f;

    [Header("Ground Snap")]
    [Tooltip("Extra height above the detected terrain surface (world units). " +
             "0 = pivot exactly on terrain. Increase if mesh sinks; decrease if it still floats.")]
    public float groundSnapOffset = 0f;

    // ── private state ────────────────────────────────────────────────
    private Animator  _animator;
    private Rigidbody _rb;

    private Vector3    _homePosition;
    private Vector3    _waypoint;
    private float      _idleTimer;
    private bool       _isIdle;
    private bool       _isAttacking;
    private float      _attackTimer;
    private Transform  _attackTarget;   // who to face while attacking

    // desired XZ unit direction — written in Update, consumed in FixedUpdate
    private Vector3 _moveDir;

    // ─────────────────────────────────────────────────────────────────
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _rb       = GetComponent<Rigidbody>();

        _homePosition = transform.position;
        PickWaypoint();
        _animator?.Play("Walk");
    }

    // ── Update: state machine + animation ────────────────────────────
    void Update()
    {
        // ── attacking ─────────────────────────────────────────────────
        if (_isAttacking)
        {
            _moveDir = Vector3.zero;

            // Continuously face the attacker (Y-axis only)
            if (_attackTarget != null)
            {
                Vector3 toTarget = _attackTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toTarget);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, targetRot, 8f * Time.deltaTime);
                }
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f) EndAttack();
            return;
        }

        // ── idling at waypoint ─────────────────────────────────────────
        if (_isIdle)
        {
            _moveDir = Vector3.zero;
            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f)
            {
                _isIdle = false;
                PickWaypoint();
                _animator?.Play("Walk");
            }
            return;
        }

        // ── walking ────────────────────────────────────────────────────
        Vector3 toWaypoint = _waypoint - transform.position;
        toWaypoint.y = 0f;                                      // compare only XZ distance

        if (toWaypoint.magnitude < arrivalDistance)
        {
            // Arrived — pause before picking the next waypoint
            _moveDir    = Vector3.zero;
            _isIdle     = true;
            _idleTimer  = idleTimeAtWaypoint;
            _animator?.Play("Idle");
        }
        else
        {
            _moveDir = toWaypoint.normalized;
        }

        // Restart walk clip manually (FBX loop-time may be off)
        if (_animator != null)
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Walk") && info.normalizedTime >= 1.0f)
                _animator.Play("Walk", 0, 0f);
        }
    }

    // ── FixedUpdate: physics movement ────────────────────────────────
    void FixedUpdate()
    {
        // ── Ground snap ───────────────────────────────────────────────
        // Cast from 100 m above to avoid hitting our own BoxCollider first.
        // Skip any hit that belongs to this GameObject (self-collision guard).
        float groundY = SampleGroundY(transform.position);
        if (groundY > float.MinValue)
        {
            float targetY  = groundY + groundSnapOffset;
            float errorY   = transform.position.y - targetY;
            if (Mathf.Abs(errorY) > 0.01f)          // dead-band to avoid micro-jitter
            {
                Vector3 snappedPos = transform.position;
                snappedPos.y = targetY;
                _rb.MovePosition(snappedPos);         // teleport to correct height
                Vector3 v = _rb.linearVelocity;
                v.y = 0f;
                _rb.linearVelocity = v;               // cancel gravity-accumulated Y vel
            }
        }

        // ── XZ locomotion ─────────────────────────────────────────────
        if (_moveDir == Vector3.zero)
        {
            // Dampen horizontal slide while preserving gravity-driven Y velocity
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            return;
        }

        // Drive XZ at walkSpeed; keep whatever Y velocity gravity produced
        _rb.linearVelocity = new Vector3(
            _moveDir.x * walkSpeed,
            _rb.linearVelocity.y,
            _moveDir.z * walkSpeed
        );

        // Smoothly face the walk direction (Y-axis rotation only; X/Z frozen by Rigidbody constraints)
        Quaternion targetRot = Quaternion.LookRotation(_moveDir);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, 6f * Time.fixedDeltaTime));
    }

    // ── Ground sampling (self-collision safe) ─────────────────────────
    /// <summary>
    /// Casts from 100 m above to find the terrain surface, skipping the crab's
    /// own collider so we never get a self-hit.
    /// </summary>
    float SampleGroundY(Vector3 pos)
    {
        Vector3 origin = new Vector3(pos.x, pos.y + 100f, pos.z);
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 200f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        // Sort closest first (RaycastAll order is undefined)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;   // skip self
            if (h.point.y > pos.y + 0.5f)           continue;   // skip surfaces above the crab (boat hull, etc.)
            return h.point.y;
        }

        return float.MinValue;   // no terrain found (should not happen in normal gameplay)
    }

    // ── Waypoint picking ─────────────────────────────────────────────
    void PickWaypoint()
    {
        Vector2 rand = Random.insideUnitCircle * wanderRadius;
        float wx = _homePosition.x + rand.x;
        float wz = _homePosition.z + rand.y;

        // Cast downward from well above the candidate XZ to find the sea-floor surface.
        // Use RaycastAll and skip any hit that is above the home position (boat hull, etc.).
        Vector3 rayOrigin = new Vector3(wx, _homePosition.y + 100f, wz);
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 300f,
                                               Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool found = false;
        foreach (RaycastHit h in hits)
        {
            if (h.point.y > _homePosition.y + 0.5f) continue;   // skip surfaces above sea floor (boat, etc.)
            _waypoint = h.point;
            found = true;
            break;
        }
        if (!found)
            _waypoint = new Vector3(wx, _homePosition.y, wz);   // fallback
    }

    // ── Public API (called by SharkBehavior) ─────────────────────────
    public void TriggerAttack(Transform attacker = null)
    {
        if (_isAttacking) return;
        _isAttacking  = true;
        _attackTimer  = attackDuration;
        _attackTarget = attacker;
        _moveDir      = Vector3.zero;
        _animator?.Play("Attack");
        AudioManager.instance?.PlayCrabAttack();   // crab attack SFX
    }

    public void StopAttack() => EndAttack();

    // ── Internals ────────────────────────────────────────────────────
    void EndAttack()
    {
        _isAttacking  = false;
        _isIdle       = false;
        _attackTarget = null;
        PickWaypoint();
        _animator?.Play("Walk");
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centre = Application.isPlaying ? _homePosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centre, wanderRadius);
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_waypoint, 0.3f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, _waypoint);
        }
    }
}
