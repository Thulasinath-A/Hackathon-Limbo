using UnityEngine;

/// <summary>
/// Player death: trigger collider + this script.
/// Crates need a solid collider too — add HazardSolidBlocker2D on the same object.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard2D : MonoBehaviour
{
    [SerializeField] PlayerRespawn2D respawnSystem;

    void Reset()
    {
        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController2D>(out _))
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
