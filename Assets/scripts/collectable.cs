using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectable : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Optional VFX prefab to spawn when collected")]
    [SerializeField] private GameObject collectEffect;
    [Tooltip("Optional SFX to play when collected")]
    [SerializeField] private AudioClip collectSfx;

    private Animator animator;
    private Collider2D col;
    private bool isCollected;
    private static readonly int CollectedHash = Animator.StringToHash("collected");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Collect();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        // Prevent further collisions
        if (col != null) col.enabled = false;

        // Play optional VFX immediately (keeps visuals while animation runs)
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Play optional SFX immediately
        if (collectSfx != null)
        {
            AudioSource.PlayClipAtPoint(collectSfx, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        // If animator exists, trigger collected animation then destroy after it finishes.
        if (animator != null)
        {
            animator.SetBool(CollectedHash, true);
            StartCoroutine(DestroyAfterAnimation());
            return;
        }

        // Fallback: no animator -> destroy immediately
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // Wait one frame so the animator can transition into the collected state
        yield return null;

        float clipLength = 0f;

        // Try to find a relevant animation clip in the controller (search by name)
        var controller = animator.runtimeAnimatorController;
        if (controller != null)
        {
            foreach (var clip in controller.animationClips)
            {
                var lower = clip.name.ToLowerInvariant();
                if (lower.Contains("collect") || lower.Contains("collected") || lower.Contains("pick"))
                {
                    clipLength = Mathf.Max(clipLength, clip.length);
                }
            }
        }

        // If not found, use the current state's length as a fallback
        if (clipLength <= 0f)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            clipLength = state.length;
        }

        // Final fallback to avoid zero wait
        if (clipLength <= 0f) clipLength = 0.5f;

        yield return new WaitForSeconds(clipLength);

        Destroy(gameObject);
    }
}