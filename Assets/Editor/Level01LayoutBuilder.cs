using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Level 01 — left-to-right teaching path:
/// safe start → jump over spikes → lower pit base + crate over spikes → jump up to exit → pit → goal.
/// Run: Hackathon Limbo → Build Level 01 Layout (open Level01 scene).
/// </summary>
public static class Level01LayoutBuilder
{
    const string ScenePath = "Assets/Scenes/Level01.unity";
    const string SpikePrefabPath = "Assets/Prefab/Spikes.prefab";
    const string PlaceholderSpritePath = "Assets/Art/PlaceholderWhite.png";
    const float GroundY = -5f;
    const float LowerBaseY = -7f;
    const float LowerWalkTopY = LowerBaseY + 0.5f;
    const float UpperSpikeY = -3.85f;
    const float LowerSpikeY = LowerWalkTopY + 0.65f;
    const float LowerCrateY = LowerWalkTopY + 0.5f;
    const float CrateStartX = 2.5f;
    const float SpawnY = -3.5f;
    const float GoalY = -3.95f;

    static readonly (string beat, string floorName, float centerX, float width, float groundY)[] Floors =
    {
        ("Beat01_Start", "Floor_Start", -16f, 12f, GroundY),
        ("Beat02_Spikes", "Floor_SpikeRunway", -7f, 6f, GroundY),
        ("Beat02_Spikes", "Floor_SpikeLanding", -2f, 4f, GroundY),
        ("Beat03_Crate", "Floor_CrateUpperEntry", -1.25f, 2f, GroundY),
        ("Beat03_Crate", "Floor_CrateDropLedge", 0.35f, 1.2f, GroundY),
        ("Beat03_Crate", "Floor_CratePitBase", 4.5f, 13f, LowerBaseY),
        ("Beat03_Crate", "Floor_AfterSpikes", 11.5f, 5f, GroundY),
        ("Beat04_Pit", "Floor_PitLeft", 15.5f, 3f, GroundY),
        ("Beat04_Pit", "Floor_PitRight", 20f, 5f, GroundY),
        ("Beat05_Goal", "Floor_Goal", 25.5f, 6f, GroundY),
    };

    static readonly (string beat, float x, float y)[] SpikePlacements =
    {
        ("Beat02_Spikes", -3f, UpperSpikeY),
        ("Beat03_Crate", 4f, LowerSpikeY),
        ("Beat03_Crate", 5.5f, LowerSpikeY),
    };

    [MenuItem("Hackathon Limbo/Build Level 01 Layout")]
    public static void BuildFromMenu()
    {
        if (Build())
        {
            EditorUtility.DisplayDialog(
                "Level 01",
                "Layout built in Level01.\n\n" +
                "Beat03: walk the short upper ledge, drop to the lower base, then push the crate on that shelf.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Level 01",
                "Build failed. Check the Console for errors.",
                "OK");
        }
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }

    static bool Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpikePrefabPath);
        if (spikePrefab == null)
        {
            Debug.LogError("Level01LayoutBuilder: Spikes prefab not found.");
            return false;
        }

        UnparentSceneEssentials();
        RemovePreviousGeneratedContent();

        var floorStamp = CreateFloorStampObject();
        if (floorStamp == null)
        {
            Debug.LogError("Level01LayoutBuilder: Could not create floor template (sprite missing?).");
            return false;
        }

        var beatRoots = CreateBeatRoots();
        foreach (var floor in Floors)
        {
            var parent = beatRoots[floor.beat];
            CreateFloor(floorStamp, parent, floor.floorName, floor.centerX, floor.width, floor.groundY);
        }

        Object.DestroyImmediate(floorStamp);

        foreach (var spike in SpikePlacements)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(spikePrefab, beatRoots[spike.beat]);
            instance.transform.position = new Vector3(spike.x, spike.y, 0f);
        }

        EnsureNamedObject("SpawnPoint", new Vector3(-19f, SpawnY, 0f), beatRoots["Beat01_Start"]);
        EnsureNamedObject("Crate", new Vector3(CrateStartX, LowerCrateY, 0f), beatRoots["Beat03_Crate"]);
        EnsureNamedObject("Goal", new Vector3(27f, GoalY, 0f), beatRoots["Beat05_Goal"]);

        var player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.SetParent(null);
            player.transform.position = new Vector3(-19f, SpawnY, 0f);
        }

        WireGameManager();
        EnsureLevelIntro();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Level01LayoutBuilder: done. Floors are named Floor_* under each Beat folder.");
        return true;
    }

    static void UnparentSceneEssentials()
    {
        foreach (var objectName in new[] { "SpawnPoint", "Crate", "Goal", "Player" })
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                go.transform.SetParent(null);
            }
        }
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

        foreach (var floorName in new[]
                 {
                     "Floor_Start", "Floor_SpikeRunway", "Floor_SpikeLanding", "Floor_CrateUpperEntry",
                     "Floor_CratePitBase", "Floor_CrateDropLedge", "Floor_CrateLane", "Floor_AfterSpikes",
                     "Floor_PitLeft",
                     "Floor_PitRight", "Floor_Goal",
                 })
        {
            var floor = GameObject.Find(floorName);
            if (floor != null)
            {
                Object.DestroyImmediate(floor);
            }
        }

        var legacyGround = GameObject.Find("Ground_Platform");
        if (legacyGround != null)
        {
            Object.DestroyImmediate(legacyGround);
        }

        var stamp = GameObject.Find("_Level01FloorStamp");
        if (stamp != null)
        {
            Object.DestroyImmediate(stamp);
        }
    }

    static GameObject CreateFloorStampObject()
    {
        var sprite = LoadPlaceholderSprite();
        if (sprite == null)
        {
            return null;
        }

        var go = new GameObject("_Level01FloorStamp");
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.black;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(12f, 1f);

        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(12f, 1f);

        ApplyGroundLayer(go);
        go.transform.position = new Vector3(0f, GroundY, 0f);
        return go;
    }

    static Sprite LoadPlaceholderSprite()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(PlaceholderSpritePath);
        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        return null;
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

    static GameObject CreateFloor(
        GameObject template,
        Transform parent,
        string name,
        float centerX,
        float width,
        float groundY)
    {
        var go = Object.Instantiate(
            template,
            new Vector3(centerX, groundY, 0f),
            Quaternion.identity,
            parent);
        go.name = name;
        go.hideFlags = HideFlags.None;
        ApplyGroundLayer(go);

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

    static void ApplyGroundLayer(GameObject go)
    {
        var groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
        {
            go.layer = groundLayer;
        }
    }

    static void EnsureNamedObject(string objectName, Vector3 position, Transform parent)
    {
        var go = GameObject.Find(objectName);
        if (go == null)
        {
            Debug.LogWarning($"Level01LayoutBuilder: {objectName} not found — create it in the scene.");
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

        var win = manager.GetComponent<LevelWin2D>();
        if (win != null)
        {
            var winSo = new SerializedObject(win);
            winSo.FindProperty("nextSceneName").stringValue = "Level02";
            winSo.ApplyModifiedPropertiesWithoutUndo();
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
