using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AdvancedPlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 12f;
    public float deceleration = 14f;

    [Header("Jumping")]
    public float jumpForce = 14f;
    public float jumpCutMultiplier = 0.5f;  
    public int maxJumps = 2;
    private int jumpCount;

    [Header("Wall Sliding & Jumping")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(12f, 14f);
    private bool isTouchingWall;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;

    [Header("Dashing")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    private bool isDashing;
    private float lastDashTime;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isFacingRight = true;

    [Header("Debug Info")]
    public Vector2 velocity;
    public bool wallSliding;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckGrounded();
        CheckWall();

        if (!isDashing)
        {
            HandleMoveInput();
            HandleJumpInput();
            HandleWallSlide();
        }

        HandleDashInput();
        FlipSprite();

        velocity = rb.velocity;  
    }

    void HandleMoveInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float targetSpeed = inputX * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);

        rb.AddForce(Vector2.right * movement);
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || jumpCount < maxJumps)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++;
            }
            else if (wallSliding)
            {
                rb.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpForce.x, wallJumpForce.y);
                Flip();
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }
    }

    void HandleWallSlide()
    {
        wallSliding = isTouchingWall && !isGrounded && rb.velocity.y < 0;

        if (wallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float direction = isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    void FlipSprite()
    {
        if (rb.velocity.x > 0.01f && !isFacingRight)
            Flip();
        else if (rb.velocity.x < -0.01f && isFacingRight)
            Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded) jumpCount = 0;
    }

    void CheckWall()
    {
        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * (isFacingRight ? 1 : -1), wallCheckDistance, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(dir * wallCheckDistance));
        }
    }
}
