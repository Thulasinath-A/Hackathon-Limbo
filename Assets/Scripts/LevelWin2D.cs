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

    InputAction restartAction;
    bool hasWon;

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
        }
    }

    public void ReachGoal()
    {
        if (hasWon)
        {
            return;
        }

        hasWon = true;

        if (gameplayUI != null)
        {
            gameplayUI.ShowWin();
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
}
