        using System.Collections.Generic;
using UnityEngine;

public class startpoint : MonoBehaviour
{
    [Header("Spawn")]
    [Tooltip("Player prefab to spawn at level start")]
    [SerializeField] private GameObject playerPrefab;
    [Tooltip("Offset from this StartPoint's position where the player will be spawned")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [Tooltip("If true, any existing GameObject with the Player tag will be destroyed before spawning")]
    [SerializeField] private bool destroyExistingPlayer = true;
    [Tooltip("Tag used to identify the player in the scene")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("If true the player will be spawned in Start(); otherwise call SpawnPlayer() manually")]
    [SerializeField] private bool spawnOnStart = true;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnPlayer();
        }
    }

    /// <summary>
    /// Spawns the player prefab at this StartPoint position + offset.
    /// Respects the destroyExistingPlayer and playerTag settings.
    /// </summary>
    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError($"[{nameof(startpoint)}] playerPrefab is not assigned on {gameObject.name}.");
            return;
        }

        // Optionally remove existing player instances
        if (destroyExistingPlayer && !string.IsNullOrEmpty(playerTag))
        {
            var existing = GameObject.FindGameObjectsWithTag(playerTag);
            foreach (var go in existing)
            {
                // don't destroy this startpoint if accidentally tagged
                if (go == gameObject) continue;
                Destroy(go);
            }
        }

        Vector3 spawnPos = transform.position + spawnOffset;
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
}
