using System.Collections;
using UnityEngine;

public class playermovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 12f;             // vertical velocity applied when jumping
    public int maxJumps = 2;                  // 1 = single jump, 2 = double jump
    public Transform groundCheck;             // assign an empty child at player's feet
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;             // set to your Ground layer

    [Header("Jump")]
    public float jumpForce = 12f;             // vertical velocity applied when jumping
    public int maxJumps = 2;                  // 1 = single jump, 2 = double jump
    public Transform groundCheck;             // assign an empty child at player's feet
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;             // set to your Ground layer

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public bool preserveVerticalVelocity = true; // set false for top-down (preserve both axes)

    Rigidbody2D rb;
    Vector2 inputDirection;
    bool isDashing = false;
    float lastDashTime = -Mathf.Infinity;

                                void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("playermovement requires a Rigidbody2D on the same GameObject.");
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(h, v).normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (!isDashing)
        {
            Vector2 targetVel = inputDirection * moveSpeed;
            if (preserveVerticalVelocity)
            {
                // Preserve current y velocity (useful for platformers with gravity)
                rb.velocity = new Vector2(targetVel.x, rb.velocity.y);
            }
            else
            {
                // Top-down style movement
                rb.velocity = targetVel;
            }
        }
    }

    IEnumerator Dash()
    {
        if (rb == null) yield break;

        isDashing = true;
        lastDashTime = Time.time;

        Vector2 dashDir = inputDirection.sqrMagnitude > 0.001f ? inputDirection : Vector2.right;
        dashDir.Normalize();

        if (preserveVerticalVelocity)
        {
            rb.velocity = new Vector2(dashDir.x * dashSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = dashDir * dashSpeed;
        }

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }
}