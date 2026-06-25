using UnityEngine;

public class CameraController : MonoBehaviour
{
	[Header("Target")]
	[Tooltip("Tag to find the player (e.g., 'Player')")]
	public string playerTag = "Player";

	[Header("Smoothing")]
	[Tooltip("Speed of camera movement towards target (0 = instant, higher = smoother)")]
	public float smoothSpeed = 5f;

	[Header("Offset")]
	[Tooltip("Offset from target position")]
	public Vector3 offset = new Vector3(0f, 1f, -10f);

	[Header("X Axis Movement")]
	[Tooltip("If true, camera only moves forward on X axis (never backward)")]
	public bool lockXAxisBackward = false;

	[Header("Bounds")]
	[Tooltip("Enable camera bounds to constrain movement")]
	public bool useBounds = false;
	[Tooltip("Minimum camera position")]
	public Vector2 minBounds = new Vector2(-10f, -5f);
	[Tooltip("Maximum camera position")]
	public Vector2 maxBounds = new Vector2(10f, 5f);

	private Camera mainCamera;
	private Transform target;
	private float initialYPosition;
	private float bottomBoundY;
	private float initialXPosition;
	private bool isInitialized = false;

	void Start()
	{
		mainCamera = GetComponent<Camera>();
		if (mainCamera == null)
		{
			Debug.LogError("CameraController must be attached to a GameObject with a Camera component.");
		}

		// Store initial positions as bounds
		initialYPosition = transform.position.y;
		bottomBoundY = initialYPosition;
		initialXPosition = transform.position.x;
		isInitialized = true;

		FindTarget();
	}

	void LateUpdate()
	{
		// Try to find target if it hasn't been found yet
		if (target == null)
		{
			FindTarget();
			return;
		}

		// Calculate desired position
		Vector3 desiredPosition = target.position + offset;

		// Apply bounds if enabled
		if (useBounds)
		{
			desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
			desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
		}

		// Enforce bottom bound (never go below initial Y position)
		desiredPosition.y = Mathf.Max(desiredPosition.y, bottomBoundY);

		// Enforce X axis forward-only movement if enabled
		if (lockXAxisBackward && isInitialized)
		{
			desiredPosition.x = Mathf.Max(desiredPosition.x, initialXPosition);
		}

		// Smooth interpolation towards desired position
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

		// Apply the new position
		transform.position = smoothedPosition;
	}

	private void FindTarget()
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
		if (playerObject != null)
		{
			target = playerObject.transform;
		}
		else
		{
			Debug.LogWarning($"CameraController: No GameObject found with tag '{playerTag}'.");
		}
	}

	// Optional: visualize bounds in the editor
	private void OnDrawGizmosSelected()
	{
		if (!useBounds) return;

		Gizmos.color = Color.green;
		Vector3 min = new Vector3(minBounds.x, minBounds.y, 0f);
		Vector3 max = new Vector3(maxBounds.x, maxBounds.y, 0f);

		// Draw rectangle outline
		Gizmos.DrawLine(new Vector3(min.x, min.y, 0f), new Vector3(max.x, min.y, 0f));
		Gizmos.DrawLine(new Vector3(max.x, min.y, 0f), new Vector3(max.x, max.y, 0f));
		Gizmos.DrawLine(new Vector3(max.x, max.y, 0f), new Vector3(min.x, max.y, 0f));
		Gizmos.DrawLine(new Vector3(min.x, max.y, 0f), new Vector3(min.x, min.y, 0f));
	}
}
