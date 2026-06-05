using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level 01 — left-to-right teaching path:
/// safe start → jump over spikes → E+crate covers spike lane → pit jump → goal.
/// Run: Hackathon Limbo → Build Level 01 Layout (open Level01 scene).
/// </summary>
public static class Level01LayoutBuilder
{
    const string ScenePath = "Assets/Scenes/Level01.unity";
    const string SpikePrefabPath = "Assets/Prefab/Spikes.prefab";
    const float GroundY = -5f;
    const float SpikeY = -3.85f;
    const float CrateY = -3.98f;
    const float SpawnY = -3.5f;
    const float GoalY = -3.95f;

    static readonly (string beat, string floorName, float centerX, float width)[] Floors =
    {
        ("Beat01_Start", "Floor_Start", -16f, 12f),
        ("Beat02_Spikes", "Floor_SpikeRunway", -7f, 6f),
        ("Beat02_Spikes", "Floor_SpikeLanding", -2f, 4f),
        ("Beat03_Crate", "Floor_CrateLane", 3f, 6f),
        ("Beat03_Crate", "Floor_AfterSpikes", 9f, 6f),
        ("Beat04_Pit", "Floor_PitLeft", 13.25f, 2.5f),
        ("Beat04_Pit", "Floor_PitRight", 20f, 5f),
        ("Beat05_Goal", "Floor_Goal", 25.5f, 6f),
    };

    static readonly (string beat, float x)[] SpikePlacements =
    {
        ("Beat02_Spikes", -3f),
        ("Beat03_Crate", 4.6f),
        ("Beat03_Crate", 5.8f),
    };

    [MenuItem("Hackathon Limbo/Build Level 01 Layout")]
    public static void BuildFromMenu()
    {
        Build();
        EditorUtility.DisplayDialog(
            "Level 01",
            "Layout built in Level01.\n\n" +
            "Beats: start → spike jump → crate covers spikes → pit → goal.\n" +
            "Save the scene if Unity prompts you.",
            "OK");
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }

    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var groundTemplate = GameObject.Find("Ground_Platform");
        if (groundTemplate == null)
        {
            Debug.LogError("Level01LayoutBuilder: Ground_Platform not found in scene.");
            return;
        }

        var spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpikePrefabPath);
        if (spikePrefab == null)
        {
            Debug.LogError("Level01LayoutBuilder: Spikes prefab not found.");
            return;
        }

        RemovePreviousGeneratedContent();
        DestroyOrphanGround(groundTemplate);

        var beatRoots = CreateBeatRoots();
        foreach (var floor in Floors)
        {
            var parent = beatRoots[floor.beat];
            CreateFloor(groundTemplate, parent, floor.floorName, floor.centerX, floor.width);
        }

        foreach (var spike in SpikePlacements)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(spikePrefab, beatRoots[spike.beat]);
            instance.transform.position = new Vector3(spike.x, SpikeY, 0f);
        }

        RepositionNamedObject("SpawnPoint", new Vector3(-19f, SpawnY, 0f), beatRoots["Beat01_Start"]);
        RepositionNamedObject("Crate", new Vector3(0.8f, CrateY, 0f), beatRoots["Beat03_Crate"]);
        RepositionNamedObject("Goal", new Vector3(27f, GoalY, 0f), beatRoots["Beat05_Goal"]);

        var player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(-19f, SpawnY, 0f);
        }

        WireGameManager();
        EnsureLevelIntro();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Level01LayoutBuilder: done.");
    }

    static void DestroyOrphanGround(GameObject template)
    {
        Object.DestroyImmediate(template);
    }

    static void RemovePreviousGeneratedContent()
    {
        foreach (var rootName in new[] { "Beat01_Start", "Beat02_Spikes", "Beat03_Crate", "Beat04_Pit", "Beat05_Goal" })
        {
            var existing = GameObject.Find(rootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        foreach (var spike in Object.FindObjectsByType<Hazard2D>(FindObjectsSortMode.None))
        {
            if (spike.gameObject.scene.IsValid())
            {
                Object.DestroyImmediate(spike.gameObject);
            }
        }
    }

    static Dictionary<string, Transform> CreateBeatRoots()
    {
        var map = new Dictionary<string, Transform>();
        foreach (var beat in new[] { "Beat01_Start", "Beat02_Spikes", "Beat03_Crate", "Beat04_Pit", "Beat05_Goal" })
        {
            var go = new GameObject(beat);
            map[beat] = go.transform;
        }

        return map;
    }

    static GameObject CreateFloor(GameObject template, Transform parent, string name, float centerX, float width)
    {
        var go = Object.Instantiate(template, parent);
        go.name = name;
        go.transform.position = new Vector3(centerX, GroundY, 0f);
        go.transform.localScale = Vector3.one;

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(width, 1f);
        }

        var collider = go.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(width, 1f);
        }

        return go;
    }

    static void RepositionNamedObject(string objectName, Vector3 position, Transform parent)
    {
        var go = GameObject.Find(objectName);
        if (go == null)
        {
            Debug.LogWarning($"Level01LayoutBuilder: {objectName} not found.");
            return;
        }

        go.transform.SetParent(parent);
        go.transform.position = position;
    }

    static void WireGameManager()
    {
        var manager = GameObject.Find("GameManager");
        if (manager == null)
        {
            return;
        }

        var respawn = manager.GetComponent<PlayerRespawn2D>();
        if (respawn != null)
        {
            var spawn = GameObject.Find("SpawnPoint")?.transform;
            var player = GameObject.Find("Player")?.transform;
            var cameraFollow = Object.FindFirstObjectByType<CameraFollow2D>();

            var so = new SerializedObject(respawn);
            if (spawn != null)
            {
                so.FindProperty("spawnPoint").objectReferenceValue = spawn;
            }

            if (player != null)
            {
                so.FindProperty("player").objectReferenceValue = player;
            }

            if (cameraFollow != null)
            {
                so.FindProperty("cameraFollow").objectReferenceValue = cameraFollow;
            }

            so.FindProperty("fallDeathY").floatValue = -12f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void EnsureLevelIntro()
    {
        var manager = GameObject.Find("GameManager");
        if (manager == null)
        {
            return;
        }

        if (manager.GetComponent<LevelIntro2D>() == null)
        {
            var intro = manager.AddComponent<LevelIntro2D>();
            var so = new SerializedObject(intro);
            so.FindProperty("levelTitle").stringValue = "Level 01";
            var ui = Object.FindFirstObjectByType<GameplayUI>();
            if (ui != null)
            {
                so.FindProperty("gameplayUI").objectReferenceValue = ui;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
