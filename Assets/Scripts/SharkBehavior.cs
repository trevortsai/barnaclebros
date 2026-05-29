using UnityEngine;

public class SharkBehavior : MonoBehaviour
{
    public enum SharkState { FollowPlayer, Retreating, WaitingAtRetreat }

    [Header("References")]
    public Transform player;
    public Transform retreatPoint;

    [Header("Movement")]
    public float followSpeed = 3f;
    public float retreatSpeed = 6f;
    public Vector3 rotationOffset = new Vector3(0f, 90f, 0f);

    [Header("Behaviour")]
    public float crabDetectRange = 8f;
    public float retreatArriveThreshold = 1.5f;
    public float retreatDuration = 6f;

    private SharkState _state = SharkState.FollowPlayer;
    private CrabBehavior _crab;
    private float _retreatTimer;

    void Start()
    {
        _crab = FindFirstObjectByType<CrabBehavior>();
    }

    void Update()
    {
        switch (_state)
        {
            case SharkState.FollowPlayer:
                FollowPlayer();
                CheckCrabProximity();
                break;

            case SharkState.Retreating:
                Retreat();
                break;

            case SharkState.WaitingAtRetreat:
                Wait();
                break;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;
        MoveToward(player.position, followSpeed);
    }

    void CheckCrabProximity()
    {
        if (_crab == null) return;
        if (Vector3.Distance(transform.position, _crab.transform.position) < crabDetectRange)
        {
            _crab.TriggerAttack();
            _state = SharkState.Retreating;
        }
    }

    void Retreat()
    {
        if (retreatPoint == null) return;
        MoveToward(retreatPoint.position, retreatSpeed);
        if (Vector3.Distance(transform.position, retreatPoint.position) < retreatArriveThreshold)
        {
            _retreatTimer = retreatDuration;
            _state = SharkState.WaitingAtRetreat;
        }
    }

    void Wait()
    {
        _retreatTimer -= Time.deltaTime;
        if (_retreatTimer <= 0f)
        {
            _crab?.StopAttack();
            _state = SharkState.FollowPlayer;
        }
    }

    void MoveToward(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look * Quaternion.Euler(rotationOffset), 5f * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (_crab != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, crabDetectRange);
        }
    }
}
