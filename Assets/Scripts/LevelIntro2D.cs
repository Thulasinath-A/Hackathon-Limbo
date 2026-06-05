using UnityEngine;

/// <summary>
/// Optional: add to GameManager per scene to show a short level title on start.
/// </summary>
public class LevelIntro2D : MonoBehaviour
{
    [SerializeField] string levelTitle = "Level 01";
    [SerializeField] GameplayUI gameplayUI;

    void Start()
    {
        if (gameplayUI == null)
        {
            gameplayUI = FindFirstObjectByType<GameplayUI>();
        }

        gameplayUI?.ShowLevelTitle(levelTitle);
    }
}
