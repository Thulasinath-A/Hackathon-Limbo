using UnityEngine;

/// <summary>
/// Crate moved when the player holds Interact (E), is on the same shelf, and holds A/D.
/// PlayerController2D drives pushes via <see cref="TryPush"/>.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableCrate2D : MonoBehaviour
{
    [SerializeField] float minMoveInput = 0.2f;
    [SerializeField] float ledgeGapCheckDistance = 0.35f;

    Rigidbody2D body;
    BoxCollider2D box;
    Vector2 spawnPosition;
    readonly RaycastHit2D[] castHits = new RaycastHit2D[16];
    int groundLayerMask;
    int hazardLayerMask;

    void Awake()
    {
        GameplayPhysics2D.Configure();

        body = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.simulated = true;

        if (box != null)
        {
            box.isTrigger = false;
        }

        var groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
        {
            gameObject.layer = groundLayer;
            groundLayerMask = 1 << groundLayer;
        }

        var hazardLayer = LayerMask.NameToLayer("Hazard");
        if (hazardLayer >= 0)
        {
            hazardLayerMask = 1 << hazardLayer;
        }

        spawnPosition = transform.position;
    }

    public void ResetToSpawn()
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.position = spawnPosition;
        transform.position = spawnPosition;
    }

    /// <summary>
    /// Attempt to move this crate from player input. Returns true if the crate moved.
    /// </summary>
    public bool TryPush(PlayerController2D player, float moveX, out Vector2 appliedDelta)
    {
        appliedDelta = Vector2.zero;
        if (player == null || !player.IsInteractHeld || !IsPlayerOnCrateShelf(player))
        {
            return false;
        }

        if (Mathf.Abs(moveX) < minMoveInput)
        {
            return false;
        }

        appliedDelta = new Vector2(moveX * player.MoveSpeed * Time.fixedDeltaTime, 0f);
        if (!TryMove(appliedDelta))
        {
            appliedDelta = Vector2.zero;
            return false;
        }

        return true;
    }

    public bool WouldBlockMove(Vector2 delta)
    {
        return !CanMove(delta);
    }

    bool TryMove(Vector2 delta)
    {
        if (!CanMove(delta))
        {
            return false;
        }

        body.MovePosition(body.position + delta);
        return true;
    }

    bool IsPlayerOnCrateShelf(PlayerController2D player)
    {
        var bounds = box.bounds;
        var feetY = player.transform.position.y;
        if (player.TryGetComponent<CapsuleCollider2D>(out var capsule))
        {
            feetY = capsule.bounds.min.y;
        }

        // Feet sit on the floor surface (above the crate collider bottom when both rest on the same floor).
        if (feetY < bounds.min.y - 0.35f || feetY > bounds.max.y + 0.45f)
        {
            return false;
        }

        var playerX = player.transform.position.x;
        var reachX = bounds.extents.x + 0.9f;
        return Mathf.Abs(playerX - bounds.center.x) <= reachX;
    }

    bool CanMove(Vector2 delta)
    {
        if (delta.sqrMagnitude <= 0f)
        {
            return false;
        }

        var direction = delta.normalized;
        var distance = delta.magnitude;
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = ~0,
            useTriggers = false
        };

        var hitCount = box.Cast(direction, filter, castHits, distance + 0.02f);
        for (var i = 0; i < hitCount; i++)
        {
            var hit = castHits[i];
            if (hit.collider == null || hit.collider == box)
            {
                continue;
            }

            if (hit.collider.GetComponentInParent<PlayerController2D>() != null)
            {
                continue;
            }

            if (hazardLayerMask != 0 && (hazardLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                continue;
            }

            // Floors/ceilings report mostly vertical normals — do not block horizontal sliding.
            if (Mathf.Abs(hit.normal.x) < 0.45f)
            {
                continue;
            }

            if (ShouldIgnoreLedgeWallBlock(direction, hit))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    bool ShouldIgnoreLedgeWallBlock(Vector2 direction, RaycastHit2D hit)
    {
        if (box == null)
        {
            return false;
        }

        var bounds = box.bounds;
        var movingRight = direction.x > 0f;
        var movingLeft = direction.x < 0f;
        if (!movingRight && !movingLeft)
        {
            return false;
        }

        if (movingRight && hit.normal.x >= -0.5f)
        {
            return false;
        }

        if (movingLeft && hit.normal.x <= 0.5f)
        {
            return false;
        }

        var frontCorner = movingRight
            ? new Vector2(bounds.max.x, bounds.min.y + 0.05f)
            : new Vector2(bounds.min.x, bounds.min.y + 0.05f);
        var hasGroundAhead = Physics2D.Raycast(
            frontCorner,
            Vector2.down,
            ledgeGapCheckDistance,
            groundLayerMask);

        return !hasGroundAhead;
    }
}
