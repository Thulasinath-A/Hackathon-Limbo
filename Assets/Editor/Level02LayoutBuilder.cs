using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Level 02 — zigzag jumps, drop alcove + two crates, aerial stepping stones, goal.
/// Run: Hackathon Limbo → Build Level 02 Layout (creates scene from Level 01 template if needed).
/// </summary>
public static class Level02LayoutBuilder
{
    const string ScenePath = "Assets/Scenes/Level02.unity";
    const string TemplateScenePath = "Assets/Scenes/Level01.unity";
    const string SpikePrefabPath = "Assets/Prefab/Spikes.prefab";
    const string PlaceholderSpritePath = "Assets/Art/PlaceholderWhite.png";
    const float GroundY = -5f;
    const float HighY = -3.5f;
    const float LowerBaseY = -7f;
    const float LowerWalkTopY = LowerBaseY + 0.5f;
    const float GroundSpikeY = -3.85f;
    const float HighSpikeY = -2.85f;
    const float LowerSpikeY = LowerWalkTopY + 0.65f;
    const float LowerCrateY = LowerWalkTopY + 0.5f;
    const float SpawnY = -3.5f;
    const float GoalY = -3.95f;
    const float CrateAX = 10f;
    const float CrateBX = 12.5f;

    static readonly (string beat, string floorName, float centerX, float width, float groundY)[] Floors =
    {
        ("Beat01_Start", "Floor_Start", -20f, 3f, GroundY),
        ("Beat02_Jumps", "Floor_Hop01", -15.5f, 2f, HighY),
        ("Beat02_Jumps", "Floor_Hop02", -12f, 2f, GroundY),
        ("Beat02_Jumps", "Floor_Hop03", -8.5f, 2f, HighY),
        ("Beat02_Jumps", "Floor_Hop04", -5f, 2f, GroundY),
        ("Beat02_Jumps", "Floor_Hop05", -1.5f, 2.5f, HighY),
        ("Beat02_Jumps", "Floor_Hop06", 2f, 2f, GroundY),
        ("Beat02_Jumps", "Floor_Hop07", 5.5f, 2f, HighY),
        ("Beat03_Crates", "Floor_CrateLip", 8.25f, 1.5f, HighY),
        ("Beat03_Crates", "Floor_Alcove", 12f, 7f, LowerBaseY),
        ("Beat03_Crates", "Floor_Climb01", 16.25f, 2f, GroundY),
        ("Beat03_Crates", "Floor_Climb02", 18.75f, 2f, HighY),
        ("Beat03_Crates", "Floor_Climb03", 21.25f, 2f, GroundY),
        ("Beat04_Aerial", "Floor_Sky01", 24.5f, 2f, HighY),
        ("Beat04_Aerial", "Floor_Sky02", 27.5f, 2f, GroundY),
        ("Beat04_Aerial", "Floor_Sky03", 30.5f, 2f, HighY),
        ("Beat04_Aerial", "Floor_Sky04", 33.5f, 2f, GroundY),
        ("Beat05_Goal", "Floor_Goal", 36.5f, 3f, HighY),
    };

    static readonly (string beat, float x, float y)[] SpikePlacements =
    {
        ("Beat02_Jumps", -5.25f, GroundSpikeY),
        ("Beat02_Jumps", -3.75f, GroundSpikeY),
        ("Beat02_Jumps", -1.75f, HighSpikeY),
        ("Beat03_Crates", 10f, LowerSpikeY),
        ("Beat03_Crates", 11.5f, LowerSpikeY),
        ("Beat03_Crates", 13.75f, LowerSpikeY),
        ("Beat04_Aerial", 25.25f, HighSpikeY),
        ("Beat04_Aerial", 31.25f, HighSpikeY),
    };

    [MenuItem("Hackathon Limbo/Create Level 02 Scene (from Level 01 template)")]
    public static void CreateSceneFromMenu()
    {
        if (EnsureSceneExists())
        {
            EditorUtility.DisplayDialog(
                "Level 02",
                "Level02.unity is ready.\n\nRun Build Level 02 Layout to generate floors, spikes, and crates.",
                "OK");
        }
    }

    [MenuItem("Hackathon Limbo/Validate Level 02 Layout")]
    public static void ValidateFromMenu()
    {
        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("Level 02", "Level02.unity does not exist yet.", "OK");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var isNewLayout = GameObject.Find("Floor_Hop01") != null && GameObject.Find("Floor_CrateRun") == null;
        EditorUtility.DisplayDialog(
            "Level 02",
            isNewLayout
                ? "Layout looks current (jump-pad Level 02)."
                : "Layout is OUT OF DATE (still old flat floors).\n\nRun: Hackathon Limbo → Build Level 02 Layout",
            "OK");
    }

    [MenuItem("Hackathon Limbo/Build Level 02 Layout")]
    public static void BuildFromMenu()
    {
        if (!EnsureSceneExists())
        {
            EditorUtility.DisplayDialog(
                "Level 02",
                "Could not create Level02.unity. Ensure Level01.unity exists.",
                "OK");
            return;
        }

        if (Build())
        {
            EditorUtility.DisplayDialog(
                "Level 02",
                "Layout built in Level02.\n\n" +
                "Vertical Level 02 built: 18 jump pads + alcove crates.\n\n" +
                "If it still looks like one long floor, you were viewing an old scene — this build replaces all Floor_* objects.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Level 02",
                "Build failed. Check the Console for errors.",
                "OK");
        }
    }

    public static void BuildFromCommandLine()
    {
        EnsureSceneExists();
        Build();
    }

    static bool EnsureSceneExists()
    {
        if (File.Exists(ScenePath))
        {
            EnsureInBuildSettings();
            return true;
        }

        if (!File.Exists(TemplateScenePath))
        {
            Debug.LogError("Level02LayoutBuilder: Level01.unity template not found.");
            return false;
        }

        if (!AssetDatabase.CopyAsset(TemplateScenePath, ScenePath))
        {
            Debug.LogError("Level02LayoutBuilder: failed to copy Level01 → Level02.");
            return false;
        }

        AssetDatabase.Refresh();
        EnsureInBuildSettings();
        Debug.Log("Level02LayoutBuilder: created Level02.unity from Level01 template.");
        return true;
    }

    static void EnsureInBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var entry in scenes)
        {
            if (entry.path == ScenePath)
            {
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static bool Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpikePrefabPath);
        if (spikePrefab == null)
        {
            Debug.LogError("Level02LayoutBuilder: Spikes prefab not found.");
            return false;
        }

        UnparentSceneEssentials();
        RemovePreviousGeneratedContent();

        var floorStamp = CreateFloorStampObject();
        if (floorStamp == null)
        {
            Debug.LogError("Level02LayoutBuilder: Could not create floor template (sprite missing?).");
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

        EnsureNamedObject("SpawnPoint", new Vector3(-20.5f, SpawnY, 0f), beatRoots["Beat01_Start"]);
        EnsureTwoCrates(
            new Vector3(CrateAX, LowerCrateY, 0f),
            new Vector3(CrateBX, LowerCrateY, 0f),
            beatRoots["Beat03_Crates"]);
        EnsureNamedObject("Goal", new Vector3(35.5f, GoalY, 0f), beatRoots["Beat05_Goal"]);

        var player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.SetParent(null);
            player.transform.position = new Vector3(-20.5f, SpawnY, 0f);
        }

        WireGameManager();
        EnsureLevelIntro();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Level02LayoutBuilder: done.");
        if (GameObject.Find("Floor_Hop01") == null)
        {
            Debug.LogError("Level02LayoutBuilder: Floor_Hop01 missing after build — check Console for errors.");
            return false;
        }

        return true;
    }

    static void EnsureTwoCrates(Vector3 posA, Vector3 posB, Transform parent)
    {
        RemoveExtraCrates();

        var crateA = GameObject.Find("Crate_A") ?? GameObject.Find("Crate");
        if (crateA == null)
        {
            Debug.LogError("Level02LayoutBuilder: no Crate or Crate_A in scene — copy from Level 01 template.");
            return;
        }

        crateA.name = "Crate_A";
        crateA.transform.SetParent(parent);
        crateA.transform.position = posA;

        var crateB = GameObject.Find("Crate_B");
        if (crateB == null)
        {
            crateB = Object.Instantiate(crateA, parent);
            crateB.name = "Crate_B";
        }

        crateB.transform.SetParent(parent);
        crateB.transform.position = posB;
    }

    static void RemoveExtraCrates()
    {
        foreach (var extraName in new[] { "Crate (1)", "Crate (2)" })
        {
            var extra = GameObject.Find(extraName);
            if (extra != null)
            {
                Object.DestroyImmediate(extra);
            }
        }

        var legacy = GameObject.Find("Crate");
        if (legacy != null && GameObject.Find("Crate_A") != null && legacy != GameObject.Find("Crate_A"))
        {
            Object.DestroyImmediate(legacy);
        }
    }

    static void UnparentSceneEssentials()
    {
        foreach (var objectName in new[] { "SpawnPoint", "Crate_A", "Crate_B", "Crate", "Goal", "Player" })
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
        foreach (var rootName in new[]
                 {
                     "Beat01_Start", "Beat02_Spikes", "Beat02_Jumps", "Beat03_Crate", "Beat03_TwoCrates",
                     "Beat03_Crates", "Beat04_Pit", "Beat04_Aerial", "Beat05_Goal",
                 })
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
                     "Floor_Start", "Floor_Hop01", "Floor_Hop02", "Floor_Hop03", "Floor_Hop04", "Floor_Hop05",
                     "Floor_Hop06", "Floor_Hop07", "Floor_CrateLip", "Floor_Alcove", "Floor_Climb01",
                     "Floor_Climb02", "Floor_Climb03", "Floor_Sky01", "Floor_Sky02", "Floor_Sky03", "Floor_Sky04",
                     "Floor_RiseA", "Floor_DipA", "Floor_RiseB", "Floor_DipB", "Floor_RiseC",
                     "Floor_StepMid", "Floor_StepHigh", "Floor_AirA", "Floor_AirB", "Floor_AirC",
                     "Floor_SpikeRunway", "Floor_SpikeLanding", "Floor_CrateRun", "Floor_CrateBridge",
                     "Floor_CrateDropLedge", "Floor_CrateAlcove", "Floor_CrateExit",
                     "Floor_CrateUpperEntry", "Floor_CratePitBase", "Floor_CrateLane", "Floor_AfterSpikes",
                     "Floor_PitLeft", "Floor_PitRight", "Floor_Goal",
                 })
        {
            var floor = GameObject.Find(floorName);
            if (floor != null)
            {
                Object.DestroyImmediate(floor);
            }
        }

        foreach (var stampName in new[] { "_Level01FloorStamp", "_Level02FloorStamp" })
        {
            var stamp = GameObject.Find(stampName);
            if (stamp != null)
            {
                Object.DestroyImmediate(stamp);
            }
        }
    }

    static GameObject CreateFloorStampObject()
    {
        var sprite = LoadPlaceholderSprite();
        if (sprite == null)
        {
            return null;
        }

        var go = new GameObject("_Level02FloorStamp");
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
        foreach (var beat in new[]
                 {
                     "Beat01_Start", "Beat02_Jumps", "Beat03_Crates", "Beat04_Aerial", "Beat05_Goal",
                 })
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
            Debug.LogWarning($"Level02LayoutBuilder: {objectName} not found — create it in the scene.");
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

        var cameraFollow = Object.FindFirstObjectByType<CameraFollow2D>();
        var respawn = manager.GetComponent<PlayerRespawn2D>();
        if (respawn != null)
        {
            var spawn = GameObject.Find("SpawnPoint")?.transform;
            var player = GameObject.Find("Player")?.transform;

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

        if (cameraFollow != null)
        {
            var camSo = new SerializedObject(cameraFollow);
            camSo.FindProperty("minPlayerFollowY").floatValue = -7f;
            camSo.ApplyModifiedPropertiesWithoutUndo();
        }

        var win = manager.GetComponent<LevelWin2D>();
        if (win != null)
        {
            var winSo = new SerializedObject(win);
            winSo.FindProperty("nextSceneName").stringValue = string.Empty;
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

        var intro = manager.GetComponent<LevelIntro2D>();
        if (intro == null)
        {
            intro = manager.AddComponent<LevelIntro2D>();
        }

        var so = new SerializedObject(intro);
        so.FindProperty("levelTitle").stringValue = "Level 02";
        var ui = Object.FindFirstObjectByType<GameplayUI>();
        if (ui != null)
        {
            so.FindProperty("gameplayUI").objectReferenceValue = ui;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
