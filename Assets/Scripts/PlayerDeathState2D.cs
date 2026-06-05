using UnityEngine;

/// <summary>
/// Handles "dead" vs alive on the player. Hazards call EnterDeath immediately;
/// respawn calls Revive. Hook Animator here later (e.g. SetTrigger("Die")).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDeathState2D : MonoBehaviour
{
    [SerializeField] PlayerController2D movement;
    [SerializeField] Animator animator;
    [SerializeField] string dieTriggerName = "Die";

    Rigidbody2D body;
    RigidbodyType2D bodyTypeBeforeDeath;
    RigidbodyConstraints2D constraintsBeforeDeath;

    public bool IsDead { get; private set; }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (movement == null)
        {
            movement = GetComponent<PlayerController2D>();
        }
    }

    public void EnterDeath()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        if (movement != null)
        {
            movement.enabled = false;
        }

        bodyTypeBeforeDeath = body.bodyType;
        constraintsBeforeDeath = body.constraints;

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;

        if (animator != null && !string.IsNullOrEmpty(dieTriggerName))
        {
            animator.SetTrigger(dieTriggerName);
        }
    }

    public void Revive()
    {
        if (!IsDead)
        {
            return;
        }

        IsDead = false;
        body.bodyType = bodyTypeBeforeDeath;
        body.constraints = constraintsBeforeDeath;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        if (movement != null)
        {
            movement.enabled = true;
        }
    }
}
