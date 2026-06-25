using UnityEngine;

public class platformcontroller: MonoBehaviour
{
    [Header("Path")]
    [Tooltip("Point A (can be empty). If null, current position is used.")]
    public Transform pointA;
    [Tooltip("Point B (can be empty). If null, current position + (1,0,0) is used.")]
    public Transform pointB;

    [Header("Movement")]
    [Tooltip("Movement speed in units/second")]
    public float speed = 3f;
    [Tooltip("If true the saw will ping-pong between A and B. If false the saw will teleport back to A after reaching B.")]
    public bool pingPong = true;
    [Tooltip("Time in seconds to wait at each endpoint")]
    public float waitAtPoint = 0.25f;
    [Tooltip("If true rotate the saw while moving")]
    public bool rotateWhileMoving = true;
    [Tooltip("Degrees per second to rotate when rotateWhileMoving is enabled")]
    public float rotateSpeed = 360f;

    [Header("Collision / Damage")]
    [Tooltip("Tag of objects that should be destroyed when they touch the saw")]
    public string destroyOnContactTag = "Player";
    [Tooltip("If true uses trigger collisions (collider.IsTrigger = true). If false uses normal collisions.")]
    public bool useTrigger = true;

    [Header("Platform Parent")]
    [Tooltip("If true, player becomes child of platform while touching it")]
    public bool useParenting = true;
    [Tooltip("Distance threshold to detach player (if player moves away from platform)")]
    public float detachDistance = 1f;

    // internal
    private Vector3 _posA;
    private Vector3 _posB;
    private Vector3 _target;
    private int _direction = 1; // 1 => A -> B, -1 => B -> A
    private float _waitTimer;

    // player parenting
    private Transform attachedPlayer;
    private Vector3 attachedPlayerLocalPos;

    private const float ArriveThreshold = 0.01f;

    void Start()
    {
        // Fallbacks for missing points
        _posA = pointA != null ? pointA.position : transform.position;
        _posB = pointB != null ? pointB.position : transform.position + Vector3.right;

        // If both points are identical, do nothing
        if (Vector3.Distance(_posA, _posB) <= Mathf.Epsilon)
        {
            enabled = false;
            return;
        }

        // Start moving from A to B
        transform.position = _posA;
        _direction = 1;
        _target = _posB;
        _waitTimer = 0f;
    }

    void Update()
    {
        // rotate visual if requested
        if (rotateWhileMoving)
        {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }

        // waiting at endpoint
        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        // move toward target
        Vector3 newPos = Vector3.MoveTowards(transform.position, _target, speed * Time.deltaTime);
        transform.position = newPos;

        // arrived?
        if (Vector3.Distance(transform.position, _target) <= ArriveThreshold)
        {
            // start wait
            if (waitAtPoint > 0f)
            {
                _waitTimer = waitAtPoint;
            }

            // swap behavior
            if (pingPong)
            {
                // reverse direction and set new target
                _direction = -_direction;
                _target = _direction == 1 ? _posB : _posA;
            }
            else
            {
                // non-pingpong: move A->B then teleport back to A after reaching B
                if (_target == _posB)
                {
                    // teleport back to A instantly after wait
                    transform.position = _posA;
                    _target = _posB;
                }
                else
                {
                    _target = _posB;
                }
            }
        }

        // Check if attached player has moved away
        if (attachedPlayer != null && useParenting)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, attachedPlayer.position);
            if (distanceToPlayer > detachDistance)
            {
                DetachPlayer();
            }
        }
    }

    // Destroy objects that contact the saw based on tag and collision mode
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;

        // Check if it's the player (don't destroy, attach instead)
        if (other.CompareTag("Player") && useParenting)
        {
            AttachPlayer(other.transform);
            return;
        }

        // Destroy objects with the specified tag
        if (string.IsNullOrEmpty(destroyOnContactTag)) return;
        if (other.CompareTag(destroyOnContactTag))
        {
            Destroy(other.gameObject);
        }
    }

    private void AttachPlayer(Transform player)
    {
        if (attachedPlayer == player) return; // Already attached

        // If a different player was attached, detach it first
        if (attachedPlayer != null)
        {
            DetachPlayer();
        }

        attachedPlayer = player;
        attachedPlayerLocalPos = player.localPosition;
        player.SetParent(transform);
    }

    private void DetachPlayer()
    {
        if (attachedPlayer == null) return;

        attachedPlayer.SetParent(null);
        attachedPlayer = null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 a = pointA != null ? pointA.position : transform.position;
        Vector3 b = pointB != null ? pointB.position : transform.position + Vector3.right;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(a, 0.08f);
        Gizmos.DrawSphere(b, 0.08f);
        Gizmos.DrawLine(a, b);
    }
}   