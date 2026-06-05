using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Basic 2D platformer movement for learning: horizontal move + jump.
/// Assign InputSystem_Actions in the Inspector and set Ground Layer on the platform.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;

    [Header("Ground check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.15f;
    [SerializeField] LayerMask groundLayer;

    [Header("Jump feel")]
    [Tooltip("Gravity while rising and holding jump.")]
    [SerializeField] float riseGravityScale = 1f;
    [Tooltip("Gravity while falling — higher = snappier landing.")]
    [SerializeField] float fallGravityScale = 2.35f;
    [Tooltip("Gravity if you release jump early while still going up.")]
    [SerializeField] float lowJumpGravityScale = 2.75f;

    [Header("Airborne wall snag")]
    [Tooltip("While in the air, do not drive into these layers (stops crate-edge sticking).")]
    [SerializeField] LayerMask wallLayers;
    [SerializeField] float wallCheckDistance = 0.08f;

    Rigidbody2D rb;
    CapsuleCollider2D bodyCollider;
    readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];
    InputAction moveAction;
    InputAction jumpAction;
    InputAction interactAction;
    bool jumpQueued;
    PlayerDeathState2D deathState;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        deathState = GetComponent<PlayerDeathState2D>();

        if (wallLayers.value == 0)
        {
            wallLayers = groundLayer | (1 << 0);
        }

        var playerMap = inputActions.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        jumpAction = playerMap.FindAction("Jump", true);
        interactAction = playerMap.FindAction("Interact", true);
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
        jumpAction.performed += OnJump;
    }

    void OnDisable()
    {
        jumpAction.performed -= OnJump;
        jumpAction.Disable();
        interactAction.Disable();
        moveAction.Disable();
    }

    /// <summary>True while Interact (E) is held and the player can act.</summary>
    public bool IsInteractHeld => CanMove() && interactAction.IsPressed();

    /// <summary>Current horizontal move input (-1..1), used for crate push while interacting.</summary>
    public float HorizontalMoveInput => CanMove() ? ReadHorizontalMove() : 0f;

    public float MoveSpeed => moveSpeed;

    void OnJump(InputAction.CallbackContext _)
    {
        if (!CanMove())
        {
            return;
        }

        jumpQueued = true;
    }

    bool CanMove()
    {
        if (deathState != null && deathState.IsDead)
        {
            return false;
        }

        return PlayerRespawn2D.Instance == null || !PlayerRespawn2D.Instance.IsRespawnPending;
    }

    void Update()
    {
        if (!CanMove())
        {
            return;
        }

        var moveX = ReadHorizontalMove();
        if (Mathf.Abs(moveX) > 0.01f)
        {
            var scale = transform.localScale;
            scale.x = moveX < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void FixedUpdate()
    {
        if (!CanMove())
        {
            return;
        }

        var moveX = ReadHorizontalMove();
        var velocityY = rb.linearVelocity.y;

        if (!IsGrounded())
        {
            if (IsBlockedHorizontally(moveX))
            {
                moveX = 0f;
            }

            var velocityX = rb.linearVelocity.x;
            if (Mathf.Abs(velocityX) > 0.01f && IsBlockedHorizontally(Mathf.Sign(velocityX)))
            {
                rb.linearVelocity = new Vector2(0f, velocityY);
            }
            else
            {
                rb.linearVelocity = new Vector2(moveX * moveSpeed, velocityY);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(moveX * moveSpeed, velocityY);
        }

        if (jumpQueued && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpQueued = false;
        }
        else if (!IsGrounded())
        {
            jumpQueued = false;
        }

        ApplyJumpGravity();
    }

    void ApplyJumpGravity()
    {
        if (IsGrounded())
        {
            rb.gravityScale = riseGravityScale;
            return;
        }

        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = fallGravityScale;
        }
        else if (!jumpAction.IsPressed())
        {
            rb.gravityScale = lowJumpGravityScale;
        }
        else
        {
            rb.gravityScale = riseGravityScale;
        }
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    /// <summary>
    /// Platformers use horizontal move only. Uses X from Move; ignores stick Y so diagonals do not slow A/D.
    /// </summary>
    float ReadHorizontalMove()
    {
        var move = moveAction.ReadValue<Vector2>();
        return move.x;
    }

    bool IsBlockedHorizontally(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f || bodyCollider == null)
        {
            return false;
        }

        var moveDirection = direction > 0f ? Vector2.right : Vector2.left;
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = wallLayers,
            useTriggers = false
        };

        var hitCount = bodyCollider.Cast(moveDirection, filter, wallHits, wallCheckDistance);
        return hitCount > 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
