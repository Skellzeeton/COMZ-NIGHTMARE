using UnityEngine;
using UnityEditor;
using System.IO;

public class LegacyPrefabSceneUpdater : EditorWindow
{
    private bool overwriteExistingPrefab = true;
    private DefaultAsset fallbackFolder;

    [MenuItem("Tools/Update Scene Prefabs (Legacy Safe)")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LegacyPrefabSceneUpdater), false, "Scene Prefab Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("Update Scene Prefab Instances", EditorStyles.boldLabel);
        overwriteExistingPrefab = EditorGUILayout.Toggle("Overwrite Existing Prefab", overwriteExistingPrefab);
        fallbackFolder = (DefaultAsset)EditorGUILayout.ObjectField("Fallback Prefab Folder", fallbackFolder, typeof(DefaultAsset), false);

        if (GUILayout.Button("Apply Changes to Selected Prefabs"))
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            int updatedCount = 0;

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject instance = selectedObjects[i];

                if (instance.scene.name == null || instance.scene.name == "")
                {
                    Debug.LogWarning("Skipping project asset: " + instance.name);
                    continue;
                }

                Object prefabAsset = PrefabUtility.GetPrefabParent(instance);
                string assetPath = AssetDatabase.GetAssetPath(prefabAsset);

                if (prefabAsset == null || string.IsNullOrEmpty(assetPath))
                {
                    prefabAsset = FindPrefabByName(instance.name);
                    if (prefabAsset == null)
                    {
                        Debug.LogWarning("No matching prefab found for: " + instance.name);
                        continue;
                    }
                }

                ReplacePrefabOptions options = overwriteExistingPrefab
                    ? ReplacePrefabOptions.ReplaceNameBased
                    : ReplacePrefabOptions.ConnectToPrefab;

                PrefabUtility.ReplacePrefab(instance, prefabAsset, options);
                Debug.Log("Updated prefab: " + AssetDatabase.GetAssetPath(prefabAsset));
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Updated or replaced " + updatedCount + " prefab assets.");
        }
    }
    
    private bool PathMatch(string assetPath, string targetName)
    {
        string fileName = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
        if (fileName.EndsWith(".prefab"))
            fileName = fileName.Substring(0, fileName.Length - 7); // remove ".prefab"
        return fileName == targetName;
    }


    private Object FindPrefabByName(string name)
    {
        if (fallbackFolder == null) return null;

        string folderPath = AssetDatabase.GetAssetPath(fallbackFolder);
        string[] guids = AssetDatabase.FindAssets(name + " t:Prefab", new[] { folderPath });

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (PathMatch(path, name))
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        return null;
    }
}
