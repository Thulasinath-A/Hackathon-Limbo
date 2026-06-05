using UnityEngine;

/// <summary>
/// Adds a solid (non-trigger) box collider on this hazard so crates and other physics objects cannot pass through.
/// Keep Hazard2D's trigger collider for player death; this handles blocking.
/// </summary>
[DisallowMultipleComponent]
public class HazardSolidBlocker2D : MonoBehaviour
{
    void Reset()
    {
        EnsureSolidCollider();
    }

    void OnValidate()
    {
        EnsureSolidCollider();
    }

    void EnsureSolidCollider()
    {
        var colliders = GetComponents<BoxCollider2D>();
        foreach (var collider in colliders)
        {
            if (!collider.isTrigger)
            {
                return;
            }
        }

        var trigger = GetComponent<BoxCollider2D>();
        var solid = gameObject.AddComponent<BoxCollider2D>();
        solid.isTrigger = false;

        if (trigger != null && trigger.isTrigger)
        {
            solid.size = trigger.size;
            solid.offset = trigger.offset;
        }
    }
}
