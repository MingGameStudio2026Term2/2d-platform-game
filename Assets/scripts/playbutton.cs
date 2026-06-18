using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class playbutton : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Name of the scene to load when this object is clicked")]
    [SerializeField] private string levelName = "level1";

    [Header("Options")]
    [Tooltip("Optional delay before loading the scene (useful for click SFX / animations)")]
    [SerializeField] private float loadDelay = 0f;
    [Tooltip("Optional click sound played when the button is pressed")]
    [SerializeField] private AudioClip clickSfx;

    private bool _activated;

    // Called when the user clicks/taps the collider (works with Collider2D).
    private void OnMouseDown()
    {
        if (_activated) return;
        _activated = true;
        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        if (clickSfx != null)
        {
            AudioSource.PlayClipAtPoint(clickSfx, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        if (loadDelay > 0f)
            yield return new WaitForSeconds(loadDelay);

        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogWarning($"[{nameof(playbutton)}] levelName is empty on {gameObject.name}.");
            yield break;
        }

        SceneManager.LoadScene(levelName);
    }
}                                           