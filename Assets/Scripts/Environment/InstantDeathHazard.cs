using UnityEngine;

public class InstantDeathHazard : MonoBehaviour
{
    [SerializeField] private int damage = 999999;
    [SerializeField] private bool makeColliderTriggerOnReset = true;

    private void Reset()
    {
        Collider2D hazardCollider = GetComponent<Collider2D>();

        if (hazardCollider != null && makeColliderTriggerOnReset)
            hazardCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KillPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        KillPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        KillPlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        KillPlayer(collision.collider);
    }

    private void KillPlayer(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();

        if (player == null)
            return;

        PlayerStats stats = player.GetComponent<PlayerStats>();

        if (stats != null && !stats.isDead)
            stats.TakeDamage(damage);
        else if (stats == null)
            player.Die();
    }
}
