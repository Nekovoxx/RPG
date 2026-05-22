using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 localPointA;
    [SerializeField] private Vector2 localPointB = new Vector2(4f, 0);
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float waitTime = 0.2f;

    private Rigidbody2D rb;
    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 target;
    private float waitTimer;
    private readonly Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();

    public void Configure(Vector2 pointAOffset, Vector2 pointBOffset, float moveSpeed, float pauseTime)
    {
        localPointA = pointAOffset;
        localPointB = pointBOffset;
        speed = moveSpeed;
        waitTime = pauseTime;
        RefreshPoints();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        RefreshPoints();
    }

    private void FixedUpdate()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (Vector2.Distance(next, target) > 0.02f)
            return;

        target = target == pointA ? pointB : pointA;
        waitTimer = waitTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player player = collision.collider.GetComponentInParent<Player>();

        if (player == null)
            return;

        Transform playerTransform = player.transform;

        if (!originalParents.ContainsKey(playerTransform))
            originalParents.Add(playerTransform, playerTransform.parent);

        playerTransform.SetParent(transform, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Player player = collision.collider.GetComponentInParent<Player>();

        if (player == null)
            return;

        Transform playerTransform = player.transform;

        if (playerTransform.parent != transform)
            return;

        originalParents.TryGetValue(playerTransform, out Transform originalParent);
        playerTransform.SetParent(originalParent, true);
        originalParents.Remove(playerTransform);
    }

    private void RefreshPoints()
    {
        pointA = (Vector2)transform.position + localPointA;
        pointB = (Vector2)transform.position + localPointB;
        target = pointB;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 a = transform.position + (Vector3)localPointA;
        Vector3 b = transform.position + (Vector3)localPointB;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.18f);
        Gizmos.DrawWireSphere(b, 0.18f);
    }
}
