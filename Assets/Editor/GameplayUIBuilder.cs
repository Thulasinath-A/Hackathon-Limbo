using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayUIBuilder
{
    const string PrefabPath = "Assets/Prefabs/UI_Gameplay.prefab";

    [MenuItem("Hackathon Limbo/Create Gameplay UI Prefab")]
    public static void CreatePrefabFromMenu()
    {
        CreatePrefab();
        EditorUtility.DisplayDialog(
            "Gameplay UI",
            "Created Assets/Prefabs/UI_Gameplay.prefab.\n\nDrag it into Level01 (and Main), then assign Gameplay UI on GameManager → Level Win 2D if needed.",
            "OK");
    }

    public static void CreatePrefab()
    {
        EnsureFolder("Assets/Prefabs");

        var root = new GameObject("UI_Gameplay");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        var winPanel = CreatePanel(root.transform, "WinPanel", new Color(0f, 0f, 0f, 0.45f));
        StretchFullScreen(winPanel.GetComponent<RectTransform>());

        var winBox = CreatePanel(winPanel.transform, "WinBox", new Color(0.12f, 0.12f, 0.13f, 0.92f));
        var winBoxRect = winBox.GetComponent<RectTransform>();
        winBoxRect.anchorMin = new Vector2(0.5f, 0.5f);
        winBoxRect.anchorMax = new Vector2(0.5f, 0.5f);
        winBoxRect.pivot = new Vector2(0.5f, 0.5f);
        winBoxRect.sizeDelta = new Vector2(480, 140);
        winBoxRect.anchoredPosition = Vector2.zero;

        var winTitle = CreateText(winBox.transform, "WinTitle", "You reached the light.", 28, FontStyle.Bold);
        var winTitleRect = winTitle.GetComponent<RectTransform>();
        winTitleRect.anchorMin = new Vector2(0.5f, 0.65f);
        winTitleRect.anchorMax = new Vector2(0.5f, 0.65f);
        winTitleRect.sizeDelta = new Vector2(440, 40);
        winTitleRect.anchoredPosition = Vector2.zero;

        var winHint = CreateText(winBox.transform, "WinHint", "Press R to play again.", 20, FontStyle.Normal);
        var winHintRect = winHint.GetComponent<RectTransform>();
        winHintRect.anchorMin = new Vector2(0.5f, 0.35f);
        winHintRect.anchorMax = new Vector2(0.5f, 0.35f);
        winHintRect.sizeDelta = new Vector2(440, 32);
        winHintRect.anchoredPosition = Vector2.zero;

        var levelTitlePanel = CreatePanel(root.transform, "LevelTitlePanel", new Color(0f, 0f, 0f, 0f));
        StretchFullScreen(levelTitlePanel.GetComponent<RectTransform>());

        var levelTitle = CreateText(levelTitlePanel.transform, "LevelTitleText", "Level 01", 36, FontStyle.Bold);
        var levelTitleRect = levelTitle.GetComponent<RectTransform>();
        levelTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        levelTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        levelTitleRect.sizeDelta = new Vector2(600, 48);
        levelTitleRect.anchoredPosition = Vector2.zero;

        winPanel.SetActive(false);
        levelTitlePanel.SetActive(false);

        var ui = root.AddComponent<GameplayUI>();
        var serialized = new SerializedObject(ui);
        serialized.FindProperty("winPanel").objectReferenceValue = winPanel;
        serialized.FindProperty("winTitleText").objectReferenceValue = winTitle;
        serialized.FindProperty("winHintText").objectReferenceValue = winHint;
        serialized.FindProperty("levelTitlePanel").objectReferenceValue = levelTitlePanel;
        serialized.FindProperty("levelTitleText").objectReferenceValue = levelTitle;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
    }

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static Text CreateText(Transform parent, string name, string content, int fontSize, FontStyle style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.9f, 0.9f, 0.92f, 1f);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }

    static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
