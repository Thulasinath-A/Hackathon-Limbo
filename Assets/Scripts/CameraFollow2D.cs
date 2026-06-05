using UnityEngine;

/// <summary>
/// Smooth 2D camera follow. Attach to Main Camera and assign the player.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new(0f, 2f, -10f);
    [SerializeField] float smoothTime = 0.25f;

    [Tooltip("When the player falls below this Y, the camera stops moving down (X still follows).")]
    [SerializeField] float minPlayerFollowY = -6f;

    Vector3 smoothVelocity;

    void Start()
    {
        SnapToTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            ComputeGoalPosition(),
            ref smoothVelocity,
            smoothTime);
    }

    /// <summary>Call after respawn so the view does not stay stuck low in the pit.</summary>
    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        transform.position = ComputeGoalPosition();
        smoothVelocity = Vector3.zero;
    }

    Vector3 ComputeGoalPosition()
    {
        var followPosition = target.position;
        if (followPosition.y < minPlayerFollowY)
        {
            followPosition.y = minPlayerFollowY;
        }

        var goal = followPosition + offset;
        goal.z = offset.z;
        return goal;
    }
}
