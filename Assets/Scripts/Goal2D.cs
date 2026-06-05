using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// End trigger — player enters to win. Uses a kinematic Rigidbody2D so triggers fire reliably.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Goal2D : MonoBehaviour
{
    [SerializeField] bool logWhenReached;

    void Reset()
    {
        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
        EnsureKinematicBody();
    }

    void Awake()
    {
        EnsureKinematicBody();
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryCompleteLevel(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryCompleteLevel(other);
    }

    void TryCompleteLevel(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController2D>(out _) &&
            other.GetComponentInParent<PlayerController2D>() == null)
        {
            return;
        }

        var win = LevelWin2D.Instance != null
            ? LevelWin2D.Instance
            : FindFirstObjectByType<LevelWin2D>();

        if (win == null)
        {
            Debug.LogWarning("Goal reached but no LevelWin2D on GameManager.");
            return;
        }

        if (logWhenReached)
        {
            Debug.Log("Goal trigger: player entered.");
        }

        win.ReachGoal();
    }

    void EnsureKinematicBody()
    {
        if (!TryGetComponent<Rigidbody2D>(out var body))
        {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.simulated = true;
    }
}
