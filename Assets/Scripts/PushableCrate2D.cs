using UnityEngine;

/// <summary>
/// Crate moved only while the player holds Interact (E), is touching it, and holds A/D.
/// Stays kinematic otherwise so walking into it does not shove it via physics.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PushableCrate2D : MonoBehaviour
{
    [SerializeField] float minMoveInput = 0.2f;

    Rigidbody2D body;
    PlayerController2D touchingPlayer;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        if (touchingPlayer == null || !touchingPlayer.IsInteractHeld)
        {
            return;
        }

        var moveX = touchingPlayer.HorizontalMoveInput;
        if (Mathf.Abs(moveX) < minMoveInput)
        {
            return;
        }

        var delta = new Vector2(
            moveX * touchingPlayer.MoveSpeed * Time.fixedDeltaTime,
            0f);
        body.MovePosition(body.position + delta);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TrySetTouchingPlayer(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TrySetTouchingPlayer(collision.collider);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (touchingPlayer == null)
        {
            return;
        }

        if (collision.collider.TryGetComponent<PlayerController2D>(out var player) && player == touchingPlayer)
        {
            touchingPlayer = null;
            return;
        }

        if (collision.collider.transform.IsChildOf(touchingPlayer.transform))
        {
            return;
        }

        if (collision.collider.GetComponentInParent<PlayerController2D>() == touchingPlayer)
        {
            touchingPlayer = null;
        }
    }

    void TrySetTouchingPlayer(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController2D>(out var player))
        {
            touchingPlayer = player;
            return;
        }

        var parentPlayer = other.GetComponentInParent<PlayerController2D>();
        if (parentPlayer != null)
        {
            touchingPlayer = parentPlayer;
        }
    }
}
