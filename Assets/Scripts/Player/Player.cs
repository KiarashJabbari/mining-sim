using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground and Wall Check")]
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Jump Feeling(idk what to name this bro)")]
    public float coyoteTime = 0.067f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private SpriteRenderer sr;
    private Animator anim;

    private float coyoteTimer;
    private string currentAnim = "";

    public bool Grounded { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Grounded = CheckGrounded();
        HandleMovement();
        HandleAnimation();
    }

    void HandleMovement()
    {
        if (Grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        float moveX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveX = 1f;

        if (moveX < 0 && CheckWalled(-1f)) moveX = 0f;
        if (moveX > 0 && CheckWalled(1f)) moveX = 0f;

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX < 0) sr.flipX = true;
        if (moveX > 0) sr.flipX = false;

        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        if (jumpHeld && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimer = 0f;
        }
    }

    void HandleAnimation()
    {
        if (anim == null) return;

        float velY = rb.linearVelocity.y;
        float velX = Mathf.Abs(rb.linearVelocity.x);

        string nextAnim;
        if (!Grounded && velY > 0.5f) nextAnim = "player_jump";
        else if (!Grounded && velY < -0.5f) nextAnim = "player_fall";
        else if (Grounded && velX > 0.1f) nextAnim = "player_walk";
        else if (Grounded) nextAnim = "player_idle";
        else nextAnim = currentAnim;

        if (nextAnim != currentAnim)
        {
            currentAnim = nextAnim;
            anim.Play(currentAnim);
        }
    }

    bool CheckGrounded()
    {
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
        return Physics2D.BoxCast(origin, new Vector2(col.bounds.size.x * 0.8f, 0.05f), 0f, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }

    bool CheckWalled(float dir)
    {
        Vector2 origin = col.bounds.center;
        return Physics2D.BoxCast(origin, new Vector2(0.05f, col.bounds.size.y * 0.6f), 0f, Vector2.right * dir, wallCheckDistance + col.bounds.size.x * 0.5f, groundLayer).collider != null;
    }

    void OnDrawGizmosSelected()
    {
        if (col == null) return;
        Gizmos.color = Color.green;
        Vector2 groundOrigin = (Vector2)transform.position + Vector2.down * (col.size.y * 0.5f);
        Gizmos.DrawWireCube(groundOrigin + Vector2.down * groundCheckDistance, new Vector3(col.size.x * 0.8f, 0.05f, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + Vector2.right * (col.size.x * 0.5f + wallCheckDistance), new Vector3(0.05f, col.size.y * 0.8f, 0));
        Gizmos.DrawWireCube((Vector2)transform.position + Vector2.left * (col.size.x * 0.5f + wallCheckDistance), new Vector3(0.05f, col.size.y * 0.8f, 0));
    }
}