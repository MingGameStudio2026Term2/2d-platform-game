using System.Collections;
    using UnityEngine;
using UnityEngine.SceneManagement;

public class endpoint : MonoBehaviour
{
    [Header("Level")]
    [Tooltip("Name of the scene to load when the player reaches this endpoint")]
    [SerializeField] private string nextLevelName = "";

    [Header("Detection")]
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("If true uses trigger collisions (collider.IsTrigger = true). If false uses normal collisions.")]
    [SerializeField] private bool useTrigger = true;

    [Header("Load")]
    [Tooltip("Delay in seconds before loading the next level (useful for animations)")]
    [SerializeField] private float loadDelay = 0f;

    // prevent multiple loads
    private bool _activated;

    private void Awake()
    {
        _activated = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;
        TryActivate(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;
        TryActivate(collision.gameObject);
    }

    private void TryActivate(GameObject other)
    {
        if (_activated) return;
        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogWarning($"[{nameof(endpoint)}] nextLevelName is not set on {gameObject.name}.");
            return;
        }

        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            _activated = true;
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        if (loadDelay > 0f)
            yield return new WaitForSeconds(loadDelay);

        // Optionally you could add fade-out or other effects here before loading.
        SceneManager.LoadScene(nextLevelName);
    }

    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.6f);
        UnityEngine.Profiling.Profiler.BeginSample("endpointGizmos"); // lightweight marker
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
