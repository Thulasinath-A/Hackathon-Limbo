using UnityEngine;

/// <summary>
/// One-time 2D physics layer rules for this project.
/// </summary>
public static class GameplayPhysics2D
{
    static bool configured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ConfigureBeforeSceneLoad()
    {
        Configure();
    }

    public static void Configure()
    {
        if (configured)
        {
            return;
        }

        var ground = LayerMask.NameToLayer("Ground");
        var hazard = LayerMask.NameToLayer("Hazard");
        if (ground >= 0 && hazard >= 0)
        {
            Physics2D.IgnoreLayerCollision(ground, hazard, true);
        }

        configured = true;
    }
}
