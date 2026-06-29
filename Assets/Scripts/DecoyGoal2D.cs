using System.Collections;
using UnityEngine;

/// <summary>
/// Fake end trigger — Sekiro-style bait: optional fake win, relocate the "goal" platform, then ambush or reveal the real goal.
/// Do not put <see cref="Goal2D"/> on the same object; wire the real win on <see cref="trueGoalRoot"/>.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DecoyGoal2D : MonoBehaviour
{
    [Header("Platform move (optional)")]
    [SerializeField] Transform platformRoot;
    [SerializeField] Vector3 relocateToWorldPosition;
    [SerializeField] float relocateDuration = 0.55f;

    [Header("Fake victory (optional)")]
    [SerializeField] bool showFakeVictory = true;
    [SerializeField] float fakeVictorySeconds = 1.35f;
    [SerializeField] string fakeWinTitle = "You reached the light.";
    [SerializeField] string fakeWinHint = string.Empty;
    [SerializeField] bool freezePlayerDuringFakeWin = true;

    [Header("After the trick")]
    [SerializeField] float delayBeforeAmbush = 0.15f;
    [SerializeField] GameObject[] activateOnAmbush;
    [SerializeField] GameObject trueGoalRoot;
    [SerializeField] bool disableDecoyTriggerAfterTrigger = true;

    [Header("Respawn")]
    [SerializeField] bool resetTrickOnPlayerRespawn = true;

    Collider2D trigger;
    Vector3 platformStartPosition;
    bool trickPlayed;
    bool isRunning;
    Coroutine routine;

    void Awake()
    {
        trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }

        if (platformRoot != null)
        {
            platformStartPosition = platformRoot.position;
        }

        if (trueGoalRoot != null)
        {
            trueGoalRoot.SetActive(false);
        }

        foreach (var hazard in activateOnAmbush)
        {
            if (hazard != null)
            {
                hazard.SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        PlayerRespawn2D.OnPlayerRespawned += HandlePlayerRespawned;
    }

    void OnDisable()
    {
        PlayerRespawn2D.OnPlayerRespawned -= HandlePlayerRespawned;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryPlayTrick(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryPlayTrick(other);
    }

    void TryPlayTrick(Collider2D other)
    {
        if (trickPlayed || isRunning)
        {
            return;
        }

        if (other.GetComponentInParent<PlayerController2D>() == null)
        {
            return;
        }

        routine = StartCoroutine(PlayTrickRoutine());
    }

    IEnumerator PlayTrickRoutine()
    {
        isRunning = true;
        trickPlayed = true;

        if (showFakeVictory && LevelWin2D.Instance != null)
        {
            LevelWin2D.Instance.BeginFakeVictory(fakeWinTitle, fakeWinHint, freezePlayerDuringFakeWin);
            yield return new WaitForSeconds(fakeVictorySeconds);
            LevelWin2D.Instance.EndFakeVictory();
        }

        if (platformRoot != null && relocateDuration > 0f)
        {
            var start = platformRoot.position;
            var end = relocateToWorldPosition;
            var elapsed = 0f;
            while (elapsed < relocateDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / relocateDuration);
                platformRoot.position = Vector3.Lerp(start, end, t);
                SyncRigidbodiesToTransforms(platformRoot);
                yield return null;
            }

            platformRoot.position = end;
            SyncRigidbodiesToTransforms(platformRoot);
        }
        else if (platformRoot != null)
        {
            platformRoot.position = relocateToWorldPosition;
            SyncRigidbodiesToTransforms(platformRoot);
        }

        if (delayBeforeAmbush > 0f)
        {
            yield return new WaitForSeconds(delayBeforeAmbush);
        }

        foreach (var hazard in activateOnAmbush)
        {
            if (hazard != null)
            {
                hazard.SetActive(true);
            }
        }

        if (trueGoalRoot != null)
        {
            trueGoalRoot.SetActive(true);
        }

        if (disableDecoyTriggerAfterTrigger && trigger != null)
        {
            trigger.enabled = false;
        }

        isRunning = false;
        routine = null;
    }

    void HandlePlayerRespawned()
    {
        if (!resetTrickOnPlayerRespawn)
        {
            LevelWin2D.Instance?.EndFakeVictory();
            return;
        }

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        isRunning = false;
        trickPlayed = false;

        LevelWin2D.Instance?.EndFakeVictory();

        if (platformRoot != null)
        {
            platformRoot.position = platformStartPosition;
            SyncRigidbodiesToTransforms(platformRoot);
        }

        foreach (var hazard in activateOnAmbush)
        {
            if (hazard != null)
            {
                hazard.SetActive(false);
            }
        }

        if (trueGoalRoot != null)
        {
            trueGoalRoot.SetActive(false);
        }

        if (trigger != null)
        {
            trigger.enabled = true;
        }
    }

    static void SyncRigidbodiesToTransforms(Transform root)
    {
        foreach (var body in root.GetComponentsInChildren<Rigidbody2D>())
        {
            body.position = body.transform.position;
        }
    }
}
