using UnityEngine;
using UnityEditor;

public class MeshFromColliderReplacer : EditorWindow
{
    [MenuItem("Tools/Replace MeshFilter with MeshCollider Mesh")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MeshFromColliderReplacer), false, "Collider Mesh Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace MeshFilter Mesh with MeshCollider Mesh", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply to Selected GameObjects"))
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            int replacedCount = 0;

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject go = selectedObjects[i];
                MeshCollider collider = go.GetComponent<MeshCollider>();
                MeshFilter filter = go.GetComponent<MeshFilter>();

                if (collider == null || collider.sharedMesh == null)
                {
                    Debug.LogWarning("Missing or invalid MeshCollider on: " + go.name);
                    continue;
                }

                if (filter == null)
                {
                    Debug.LogWarning("Missing MeshFilter on: " + go.name);
                    continue;
                }

                filter.sharedMesh = collider.sharedMesh;
                replacedCount++;
                Debug.Log("Replaced mesh on: " + go.name);
            }

            Debug.Log("Mesh replacement complete. Total updated: " + replacedCount);
        }
    }
}