using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-time setup for a blank 2D learning project (no finished level).
/// </summary>
public static class LimboLearningBootstrap
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string PlaceholderSpritePath = "Assets/Art/PlaceholderWhite.png";

    [MenuItem("Hackathon Limbo/Initialize Learning Project (2D)")]
    public static void InitializeFromMenu()
    {
        Initialize();
        EditorUtility.DisplayDialog(
            "Hackathon Limbo",
            "2D learning project is ready.\n\nOpen Assets/Scenes/Main.unity and start building.",
            "OK");
    }

    public static void InitializeFromCommandLine()
    {
        Initialize();
    }

    static void Initialize()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Scripts");
        EnsureFolder("Assets/Art");
        EnsurePlaceholderSprite();
        EnsureMainScene();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        var leaf = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent ?? "Assets", leaf);
    }

    static void EnsurePlaceholderSprite()
    {
        if (File.Exists(PlaceholderSpritePath))
        {
            return;
        }

        var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        var pixels = new Color[32 * 32];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(PlaceholderSpritePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(PlaceholderSpritePath);
        var importer = AssetImporter.GetAtPath(PlaceholderSpritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
    }

    static void EnsureMainScene()
    {
        if (File.Exists(ScenePath))
        {
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var camera = Camera.main;
        if (camera != null)
        {
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.11f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        var ground = new GameObject("Ground_Platform");
        var renderer = ground.AddComponent<SpriteRenderer>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderSpritePath);
        renderer.sprite = sprite;
        renderer.color = Color.black;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(12f, 1f);
        ground.transform.position = new Vector3(0f, -2f, 0f);

        var collider = ground.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(12f, 1f);

        EditorSceneManager.SaveScene(scene, ScenePath);
    }
}
