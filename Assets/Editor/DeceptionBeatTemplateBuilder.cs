using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Adds a Sekiro-style decoy end beat to the open scene (moving platform + fake win + ambush + real goal).
/// </summary>
public static class DeceptionBeatTemplateBuilder
{
    const string PlaceholderSpritePath = "Assets/Art/PlaceholderWhite.png";
    const string SpikePrefabPath = "Assets/Prefab/Spikes.prefab";

    [MenuItem("Hackathon Limbo/Add Deception End Beat (to open scene)")]
    public static void AddFromMenu()
    {
        if (AddBeat(new Vector3(34f, -3.5f, 0f)))
        {
            EditorUtility.DisplayDialog(
                "Deception beat",
                "Added Beat_Deception near x=34.\n\n" +
                "Decoy: walk into Goal_Decoy trigger.\n" +
                "Real win: Goal_Real after the trick.\n\n" +
                "Tune DecoyGoal2D on Goal_Decoy (move target, ambush spike).",
                "OK");
        }
    }

    public static bool AddBeat(Vector3 decoyPlatformCenter)
    {
        var sprite = LoadPlaceholderSprite();
        if (sprite == null)
        {
            Debug.LogError("DeceptionBeatTemplateBuilder: placeholder sprite missing.");
            return false;
        }

        var beatRoot = new GameObject("Beat_Deception");
        var groundLayer = LayerMask.NameToLayer("Ground");

        var decoyFloor = CreateFloor(
            beatRoot.transform,
            "Floor_DecoyEnd",
            decoyPlatformCenter,
            new Vector2(3f, 1f),
            sprite,
            groundLayer);

        var decoyTrigger = new GameObject("Goal_Decoy");
        decoyTrigger.transform.SetParent(beatRoot.transform);
        decoyTrigger.transform.position = decoyPlatformCenter + new Vector3(0f, 0.25f, 0f);
        var trigger = decoyTrigger.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(2f, 2f);

        var decoy = decoyTrigger.AddComponent<DecoyGoal2D>();
        var decoySo = new SerializedObject(decoy);
        decoySo.FindProperty("platformRoot").objectReferenceValue = decoyFloor.transform;
        decoySo.FindProperty("relocateToWorldPosition").vector3Value =
            decoyPlatformCenter + new Vector3(6f, -1.5f, 0f);
        decoySo.FindProperty("relocateDuration").floatValue = 0.6f;
        decoySo.FindProperty("showFakeVictory").boolValue = true;
        decoySo.FindProperty("fakeVictorySeconds").floatValue = 1.4f;
        decoySo.FindProperty("delayBeforeAmbush").floatValue = 0.2f;
        decoySo.FindProperty("resetTrickOnPlayerRespawn").boolValue = true;

        var spikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpikePrefabPath);
        GameObject ambush = null;
        if (spikePrefab != null)
        {
            ambush = (GameObject)PrefabUtility.InstantiatePrefab(spikePrefab, beatRoot.transform);
            ambush.name = "Ambush_Spike";
            ambush.transform.position = decoyPlatformCenter + new Vector3(0.5f, -2.85f, 0f);
            ambush.SetActive(false);
        }

        var trueRoot = new GameObject("TrueGoal_Root");
        trueRoot.transform.SetParent(beatRoot.transform);
        var trueCenter = decoyPlatformCenter + new Vector3(10f, 0f, 0f);
        trueRoot.transform.position = trueCenter;

        CreateFloor(
            trueRoot.transform,
            "Floor_TrueEnd",
            trueCenter,
            new Vector2(3f, 1f),
            sprite,
            groundLayer);

        var realGoal = new GameObject("Goal_Real");
        realGoal.transform.SetParent(trueRoot.transform);
        realGoal.transform.position = trueCenter + new Vector3(0f, 0.25f, 0f);
        var goalCol = realGoal.AddComponent<BoxCollider2D>();
        goalCol.isTrigger = true;
        goalCol.size = new Vector2(1.5f, 2f);
        realGoal.AddComponent<Goal2D>();

        trueRoot.SetActive(false);

        if (ambush != null)
        {
            decoySo.FindProperty("activateOnAmbush").arraySize = 1;
            decoySo.FindProperty("activateOnAmbush").GetArrayElementAtIndex(0).objectReferenceValue = ambush;
        }

        decoySo.FindProperty("trueGoalRoot").objectReferenceValue = trueRoot;
        decoySo.ApplyModifiedPropertiesWithoutUndo();

        var legacyGoal = GameObject.Find("Goal");
        if (legacyGoal != null && legacyGoal.GetComponent<Goal2D>() != null)
        {
            Debug.LogWarning(
                "DeceptionBeatTemplateBuilder: scene still has a plain 'Goal' — disable or remove it so only Goal_Real wins.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return true;
    }

    static GameObject CreateFloor(
        Transform parent,
        string name,
        Vector3 center,
        Vector2 size,
        Sprite sprite,
        int groundLayer)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = center;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.black;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;

        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = size;

        if (groundLayer >= 0)
        {
            go.layer = groundLayer;
        }

        return go;
    }

    static Sprite LoadPlaceholderSprite()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(PlaceholderSpritePath);
        foreach (var asset in assets)
        {
            if (asset is Sprite s)
            {
                return s;
            }
        }

        return null;
    }
}
