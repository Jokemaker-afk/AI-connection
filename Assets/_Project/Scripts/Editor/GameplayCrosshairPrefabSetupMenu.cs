using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameplayCrosshairPrefabSetupMenu
{
    const string PrefabFolder = "Assets/_Project/Prefabs/Shared/UI";
    const string PrefabPath = PrefabFolder + "/PF_Gameplay_Crosshair_01.prefab";
    const string ResourcesFolder = "Assets/_Project/Resources/UI";
    const string ResourcesPrefabPath = ResourcesFolder + "/PF_Gameplay_Crosshair_01.prefab";

    [MenuItem("Tools/UI/Create Gameplay Crosshair Prefab")]
    public static void CreateGameplayCrosshairPrefab()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(ResourcesFolder);

        GameObject tempRoot = new GameObject("PF_Gameplay_Crosshair_01", typeof(RectTransform));
        try
        {
            GameplayCrosshairBuilder.CreateCrosshairRoot(tempRoot.transform);
            ApplySpritesToHierarchy(tempRoot.transform);

            SavePrefab(tempRoot, PrefabPath);
            SavePrefab(tempRoot, ResourcesPrefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Gameplay Crosshair",
                "已创建准星 Prefab：\n" + PrefabPath + "\n" + ResourcesPrefabPath,
                "OK");
        }
        finally
        {
            Object.DestroyImmediate(tempRoot);
        }
    }

    static void SavePrefab(GameObject source, string path)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            PrefabUtility.SaveAsPrefabAsset(source, path);
            return;
        }

        PrefabUtility.SaveAsPrefabAsset(source, path);
    }

    static void EnsureFolder(string assetFolder)
    {
        if (AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(assetFolder)?.Replace('\\', '/');
        string leaf = Path.GetFileName(assetFolder);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, leaf);
    }

    static void ApplySpritesToHierarchy(Transform root)
    {
        var images = root.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            GameplayUiSpriteUtility.ApplyWhiteSprite(images[i]);
        }
    }
}
