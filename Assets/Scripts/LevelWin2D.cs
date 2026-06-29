using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to GameManager. Handles win state and restart.
/// </summary>
public class LevelWin2D : MonoBehaviour
{
    public static LevelWin2D Instance { get; private set; }

    [SerializeField] PlayerController2D player;
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] GameplayUI gameplayUI;
    [Tooltip("Optional. Scene name without path, e.g. Level02. Loaded with N or Enter after win.")]
    [SerializeField] string nextSceneName;

    InputAction restartAction;
    bool hasWon;
    bool fakeVictoryActive;

    public bool HasRealWin => hasWon;

    void Awake()
    {
        Instance = this;
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController2D>();
        }

        if (gameplayUI == null)
        {
            gameplayUI = FindFirstObjectByType<GameplayUI>();
        }

        if (inputActions != null)
        {
            restartAction = inputActions.FindAction("Restart", false);
            if (restartAction == null)
            {
                var playerMap = inputActions.FindActionMap("Player", false);
                restartAction = playerMap?.FindAction("Restart", false);
            }
        }
    }

    void OnEnable()
    {
        restartAction?.Enable();
    }

    void OnDisable()
    {
        restartAction?.Disable();
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
        if (!hasWon)
        {
            return;
        }

        var restartPressed = restartAction != null && restartAction.WasPressedThisFrame();
        if (!restartPressed && Keyboard.current != null)
        {
            restartPressed = Keyboard.current.rKey.wasPressedThisFrame;
        }

        if (restartPressed)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        var nextPressed = Keyboard.current != null &&
                          (Keyboard.current.nKey.wasPressedThisFrame ||
                           Keyboard.current.enterKey.wasPressedThisFrame);
        if (nextPressed)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void ReachGoal()
    {
        if (hasWon || fakeVictoryActive)
        {
            return;
        }

        hasWon = true;
        fakeVictoryActive = false;

        if (gameplayUI != null)
        {
            var hint = string.IsNullOrEmpty(nextSceneName)
                ? "Press R to play again."
                : "Press N for next level · R to retry.";
            gameplayUI.ShowWin(hint: hint);
        }
        else
        {
            Debug.LogWarning("LevelWin2D: no GameplayUI in scene — add UI_Gameplay prefab.");
        }

        if (player != null)
        {
            player.enabled = false;
            if (player.TryGetComponent<Rigidbody2D>(out var body))
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }
    }

    /// <summary>Fake level clear — UI only; player can still be ambushed unless frozen.</summary>
    public void BeginFakeVictory(string title, string hint, bool freezePlayer)
    {
        if (hasWon)
        {
            return;
        }

        fakeVictoryActive = true;

        if (gameplayUI != null)
        {
            gameplayUI.ShowWin(
                title,
                string.IsNullOrEmpty(hint) ? "..." : hint);
        }

        if (!freezePlayer || player == null)
        {
            return;
        }

        player.enabled = false;
        if (player.TryGetComponent<Rigidbody2D>(out var body))
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    /// <summary>Dismiss fake win and restore control (real win unchanged).</summary>
    public void EndFakeVictory()
    {
        if (hasWon)
        {
            return;
        }

        fakeVictoryActive = false;
        gameplayUI?.DismissWin();

        if (player != null && player.enabled == false)
        {
            player.enabled = true;
        }
    }
}
