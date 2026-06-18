using UnityEngine;

public class killzone : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string targetTag = "Player";
    [Tooltip("If true uses trigger collisions (collider.IsTrigger = true). If false uses normal collisions.")]
    [SerializeField] private bool useTrigger = true;
    [Tooltip("Optional delay (seconds) before destroying the object that touched the killzone")]
    [SerializeField] private float destroyDelay = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;
        TryDestroy(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;
        TryDestroy(collision.gameObject);
    }

    private void TryDestroy(GameObject other)
    {
        if (string.IsNullOrEmpty(targetTag)) return;
        if (!other.CompareTag(targetTag)) return;

        if (destroyDelay <= 0f)
            Destroy(other);
        else
            Destroy(other, destroyDelay);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.6f);
    }
}               