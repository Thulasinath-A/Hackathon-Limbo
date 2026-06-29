using System.Collections;
using UnityEngine;

/// <summary>
/// Sends the player back to a spawn point when they fall below fallDeathY.
/// Put this on an empty GameManager object in the scene.
/// </summary>
public class PlayerRespawn2D : MonoBehaviour
{
    public static PlayerRespawn2D Instance { get; private set; }

    [SerializeField] Transform player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float fallDeathY = -8f;
    [SerializeField] CameraFollow2D cameraFollow;

    [Header("Hazard respawn")]
    [SerializeField] float hazardRespawnDelay = 0.5f;

    [Header("Level reset")]
    [Tooltip("Leave empty to auto-find all PushableCrate2D in the scene.")]
    [SerializeField] PushableCrate2D[] resetCrates;

    Rigidbody2D playerBody;
    PlayerController2D playerController;
    PlayerDeathState2D playerDeath;
    Coroutine hazardRespawnRoutine;
    RigidbodyType2D bodyTypeBeforeHazardFreeze;
    bool frozeBodyWithoutDeathComponent;

    public bool IsRespawnPending { get; private set; }

    public static event System.Action OnPlayerRespawned;

    void Awake()
    {
        Instance = this;

        if (player != null)
        {
            playerBody = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<PlayerController2D>();
            playerDeath = player.GetComponent<PlayerDeathState2D>();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        EnsureCrateList();
        PlacePlayerAtSpawn(snapCamera: true);
    }

    void EnsureCrateList()
    {
        if (resetCrates != null && resetCrates.Length > 0)
        {
            return;
        }

        resetCrates = FindObjectsByType<PushableCrate2D>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (player == null || spawnPoint == null || IsRespawnPending)
        {
            return;
        }

        if (player.position.y < fallDeathY)
        {
            RespawnPlayer();
        }
    }

    /// <summary>Falls — respawn immediately.</summary>
    public void RespawnPlayer()
    {
        if (player == null || spawnPoint == null)
        {
            return;
        }

        if (hazardRespawnRoutine != null)
        {
            StopCoroutine(hazardRespawnRoutine);
            hazardRespawnRoutine = null;
        }

        IsRespawnPending = false;
        UnfreezePlayer();
        ApplyRespawn();
        OnPlayerRespawned?.Invoke();
    }

    /// <summary>Hazards — freeze immediately, then respawn after a short delay.</summary>
    public void RespawnFromHazard()
    {
        if (player == null || spawnPoint == null || IsRespawnPending)
        {
            return;
        }

        FreezePlayerImmediately();
        hazardRespawnRoutine = StartCoroutine(HazardRespawnRoutine());
    }

    void FreezePlayerImmediately()
    {
        IsRespawnPending = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
        }

        if (playerDeath != null)
        {
            playerDeath.EnterDeath();
            return;
        }

        if (playerBody != null)
        {
            bodyTypeBeforeHazardFreeze = playerBody.bodyType;
            playerBody.bodyType = RigidbodyType2D.Kinematic;
            frozeBodyWithoutDeathComponent = true;
        }
    }

    void UnfreezePlayer()
    {
        if (playerDeath != null)
        {
            playerDeath.Revive();
        }
        else if (playerBody != null && frozeBodyWithoutDeathComponent)
        {
            playerBody.bodyType = bodyTypeBeforeHazardFreeze;
            frozeBodyWithoutDeathComponent = false;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    IEnumerator HazardRespawnRoutine()
    {
        var elapsed = 0f;
        while (elapsed < hazardRespawnDelay)
        {
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.angularVelocity = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyRespawn();
        UnfreezePlayer();
        IsRespawnPending = false;
        hazardRespawnRoutine = null;
        OnPlayerRespawned?.Invoke();
    }

    void ApplyRespawn()
    {
        ResetCrates();
        PlacePlayerAtSpawn(snapCamera: true);
    }

    void ResetCrates()
    {
        EnsureCrateList();
        if (resetCrates == null)
        {
            return;
        }

        foreach (var crate in resetCrates)
        {
            if (crate != null)
            {
                crate.ResetToSpawn();
            }
        }
    }

    void PlacePlayerAtSpawn(bool snapCamera)
    {
        if (player == null || spawnPoint == null)
        {
            return;
        }

        player.position = spawnPoint.position;
        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
        }

        if (!snapCamera)
        {
            return;
        }

        if (cameraFollow == null)
        {
            cameraFollow = Camera.main != null
                ? Camera.main.GetComponent<CameraFollow2D>()
                : null;
        }

        cameraFollow?.SnapToTarget();
    }
}
