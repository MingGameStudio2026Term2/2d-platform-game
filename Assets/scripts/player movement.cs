using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	public float speed = 5f;

	[Header("Jump")]
	public float jumpForce = 7f;
	[Tooltip("Maximum number of jumps allowed before touching ground (set 2 for double jump)")]
	public int maxJumps = 2;

	[Header("Wall")]
	[Tooltip("Layers considered walls")]
	public LayerMask wallLayer;
	[Tooltip("Distance for side raycast to detect walls")]
	public float wallCheckDistance = 0.6f;
	[Tooltip("Downward speed while sliding on a wall (negative value)")]
	public float wallSlideSpeed = -2f;
	[Tooltip("Vertical force applied when performing a wall jump")]
	public float wallJumpUpForce = 7f;
	[Tooltip("Horizontal bounce force away from the wall when wall-jumping")]
	public float wallBounceHorizontal = 4f;
	[Tooltip("Time during which player input won't override the wall-jump bounce")]
	public float wallJumpControlLock = 0.15f;

	private Rigidbody2D rb;
	private Animator animator;
	private bool isGrounded;
	private bool facingRight = true;
	private int jumpsRemaining;

	// Wall state
	private bool isTouchingWall;
	// +1 = wall on right, -1 = wall on left
	private int wallSide = 0;

	// Input lock timer after wall jump
	private float wallJumpLockTimer;

	private static readonly int IsRunningHash = Animator.StringToHash("isrunning");
	private static readonly int IsJumpingHash = Animator.StringToHash("isjumping");
	private static readonly int IsDoubleJumpingHash = Animator.StringToHash("isdoublejumping");
	private static readonly int VelocityYHash = Animator.StringToHash("velocityY");
	private static readonly int IsOnGroundHash = Animator.StringToHash("isonground");

	private const float RunThreshold = 0.01f;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		facingRight = transform.localScale.x >= 0f;
		jumpsRemaining = maxJumps;

		if (animator != null)
		{
			animator.SetBool(IsOnGroundHash, isGrounded);
			animator.SetBool(IsJumpingHash, false);
			animator.SetBool(IsDoubleJumpingHash, false);
			animator.SetFloat(VelocityYHash, 0f);
		}
	}

	void Update()
	{
		// Decrease wall-jump input lock timer
		if (wallJumpLockTimer > 0f)
		{
			wallJumpLockTimer -= Time.deltaTime;
		}

		float moveInput = Input.GetAxis("Horizontal");

		// Only allow player horizontal control when the lock isn't active
		if (wallJumpLockTimer <= 0f)
		{
			rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

			// Flip character when changing horizontal direction (only when input control is allowed)
			if (moveInput > RunThreshold && !facingRight)
			{
				Flip();
			}
			else if (moveInput < -RunThreshold && facingRight)
			{
				Flip();
			}
		}
		// else: do not overwrite rb.velocity.x so bounce remains intact

		// Update animator values every frame (use actual velocity.x to determine running)
		bool isRunning = Mathf.Abs(rb.velocity.x) > RunThreshold;
		if (animator != null)
		{
			animator.SetBool(IsRunningHash, isRunning);
			animator.SetFloat(VelocityYHash, rb.velocity.y);
			animator.SetBool(IsOnGroundHash, isGrounded);
		}

		// Wall detection
		CheckWall();

		// Wall slide: while touching wall and falling, slow down fall
		bool shouldWallSlide = isTouchingWall && !isGrounded && rb.velocity.y < 0f;
		if (shouldWallSlide)
		{
			rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, wallSlideSpeed));
		}

		// Jump input handling
		if (Input.GetKeyDown(KeyCode.Space))
		{
			// If touching wall (and not grounded) perform wall jump that bounces away
			if (isTouchingWall && !isGrounded)
			{
				PerformWallJump();
				return;
			}

			// Normal / double jump handling
			if (jumpsRemaining > 0)
			{
				bool isDoubleJump = !isGrounded;
				if (animator != null)
				{
					animator.SetBool(IsJumpingHash, !isDoubleJump);
					animator.SetBool(IsDoubleJumpingHash, isDoubleJump);
					animator.SetBool(IsOnGroundHash, false);
				}

				rb.velocity = new Vector2(rb.velocity.x, jumpForce);
				jumpsRemaining--;
				isGrounded = false;
			}
		}
	}

	private void CheckWall()
	{
		// Raycasts from center to left/right to check walls
		RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
		RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

		if (hitRight.collider != null)
		{
			isTouchingWall = true;
			wallSide = +1;
		}
		else if (hitLeft.collider != null)
		{
			isTouchingWall = true;
			wallSide = -1;
		}
		else
		{
			isTouchingWall = false;
			wallSide = 0;
		}
	}

	private void PerformWallJump()
	{
		// Apply upward force and a small horizontal bounce away from the wall
		int awayDir = -wallSide; // if wall is on right(+1), awayDir = -1 (left), and vice-versa
		float horizontal = awayDir * wallBounceHorizontal;
		float vertical = wallJumpUpForce;

		rb.velocity = new Vector2(horizontal, vertical);

		// Lock player horizontal input for a short interval so bounce isn't immediately overridden
		wallJumpLockTimer = wallJumpControlLock;

		// After wall-jumping we allow one more mid-air jump (optional): set remaining jumps to maxJumps - 1
		jumpsRemaining = Mathf.Max(0, maxJumps - 1);
		isGrounded = false;

		// Update animator
		if (animator != null)
		{
			animator.SetBool(IsJumpingHash, true);
			animator.SetBool(IsDoubleJumpingHash, false);
			animator.SetBool(IsOnGroundHash, false);
			animator.SetFloat(VelocityYHash, rb.velocity.y);
		}

		// Flip to face jump direction if needed
		if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
		{
			Flip();
		}
	}

	private void Flip()
	{
		facingRight = !facingRight;
		Vector3 scale = transform.localScale;
		scale.x = -scale.x;
		transform.localScale = scale;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Ground"))
		{
			isGrounded = true;
			jumpsRemaining = maxJumps;

			if (animator != null)
			{
				animator.SetBool(IsOnGroundHash, true);
				animator.SetBool(IsJumpingHash, false);
				animator.SetBool(IsDoubleJumpingHash, false);
				animator.SetFloat(VelocityYHash, rb.velocity.y);
			}
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Ground"))
		{
			isGrounded = false;
			if (animator != null) animator.SetBool(IsOnGroundHash, false);
		}
	}

	// Optional: visualize wall check in the editor
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Vector3 origin = transform.position;
		Gizmos.DrawLine(origin, origin + Vector3.right * wallCheckDistance);
		Gizmos.DrawLine(origin, origin + Vector3.left * wallCheckDistance);
	}
}
