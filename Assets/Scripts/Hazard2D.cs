using UnityEngine;

/// <summary>
/// Player death: trigger collider + this script.
/// Crates need a solid collider too — add HazardSolidBlocker2D on the same object.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard2D : MonoBehaviour
{
    [SerializeField] PlayerRespawn2D respawnSystem;

    void Awake()
    {
        if (respawnSystem == null)
        {
            respawnSystem = FindFirstObjectByType<PlayerRespawn2D>();
        }
    }

    void Reset()
    {
        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHazardHit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryHazardHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHazardHit(collision.collider);
    }

    void TryHazardHit(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController2D>() == null)
        {
            return;
        }

        if (respawnSystem == null)
        {
            respawnSystem = FindFirstObjectByType<PlayerRespawn2D>();
        }

        respawnSystem?.RespawnFromHazard();
    }
}
