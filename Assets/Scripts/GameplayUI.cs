using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable gameplay overlay (win, optional level title). Lives on UI_Gameplay prefab.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance { get; private set; }

    [SerializeField] GameObject winPanel;
    [SerializeField] Text winTitleText;
    [SerializeField] Text winHintText;
    [SerializeField] GameObject levelTitlePanel;
    [SerializeField] Text levelTitleText;
    [SerializeField] float levelTitleDuration = 2.5f;

    float levelTitleTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideWin();
        if (levelTitlePanel != null)
        {
            levelTitlePanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (levelTitlePanel == null || !levelTitlePanel.activeSelf)
        {
            return;
        }

        levelTitleTimer -= Time.deltaTime;
        if (levelTitleTimer <= 0f)
        {
            levelTitlePanel.SetActive(false);
        }
    }

    public void ShowWin(
        string title = "You reached the light.",
        string hint = "Press R to play again.")
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winTitleText != null)
        {
            winTitleText.text = title;
        }

        if (winHintText != null)
        {
            winHintText.text = hint;
        }
    }

    public void HideWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void DismissWin()
    {
        HideWin();
    }

    public void ShowLevelTitle(string title)
    {
        if (levelTitlePanel == null || levelTitleText == null)
        {
            return;
        }

        levelTitleText.text = title;
        levelTitlePanel.SetActive(true);
        levelTitleTimer = levelTitleDuration;
    }
}
