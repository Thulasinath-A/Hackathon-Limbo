using UnityEngine;
using UnityEngine.SceneManagement;

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

        gameplayUI?.ShowLevelTitle(ResolveTitle());
    }

    string ResolveTitle()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Length > 5 && sceneName.StartsWith("Level")
            && int.TryParse(sceneName.Substring(5), out var levelNumber))
        {
            return $"Level {levelNumber:00}";
        }

        return levelTitle;
    }
}
