using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshRebuilderEditor : EditorWindow
{
    private Mesh sourceMesh;
    private bool overwriteOriginal = false;

    [MenuItem("Tools/Mesh Rebuilder")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MeshRebuilderEditor), false, "Mesh Rebuilder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rebuild Legacy Mesh", EditorStyles.boldLabel);
        sourceMesh = (Mesh)EditorGUILayout.ObjectField("Source Mesh", sourceMesh, typeof(Mesh), false);
        overwriteOriginal = EditorGUILayout.Toggle("Replace Original Mesh", overwriteOriginal);

        if (GUILayout.Button("Rebuild Mesh"))
        {
            Object[] targets = Selection.GetFiltered(typeof(Mesh), SelectionMode.Assets);
            if (sourceMesh != null)
            {
                RebuildMesh(sourceMesh);
            }

            foreach (Object obj in targets)
            {
                Mesh mesh = obj as Mesh;
                if (mesh != null && mesh != sourceMesh)
                {
                    RebuildMesh(mesh);
                }
            }
        }
    }

    private void RebuildMesh(Mesh mesh)
    {
        string assetPath = AssetDatabase.GetAssetPath(mesh);
        if (string.IsNullOrEmpty(assetPath)) return;

        Mesh newMesh = new Mesh();
        newMesh.name = mesh.name + "_Rebuilt";
        newMesh.Clear();
        
        if (mesh.vertices != null && mesh.vertexCount > 0) newMesh.vertices = mesh.vertices;
        if (mesh.normals != null && mesh.normals.Length == mesh.vertexCount) newMesh.normals = mesh.normals;
        if (mesh.tangents != null && mesh.tangents.Length == mesh.vertexCount) newMesh.tangents = mesh.tangents;
        if (mesh.uv != null && mesh.uv.Length == mesh.vertexCount) newMesh.uv = mesh.uv;
        if (mesh.uv2 != null && mesh.uv2.Length == mesh.vertexCount) newMesh.uv2 = mesh.uv2;
        if (mesh.uv3 != null && mesh.uv3.Length == mesh.vertexCount) newMesh.uv3 = mesh.uv3;
        if (mesh.uv4 != null && mesh.uv4.Length == mesh.vertexCount) newMesh.uv4 = mesh.uv4;
        if (mesh.boneWeights != null && mesh.boneWeights.Length == mesh.vertexCount) newMesh.boneWeights = mesh.boneWeights;
        if (mesh.bindposes != null && mesh.bindposes.Length > 0) newMesh.bindposes = mesh.bindposes;
        
        newMesh.subMeshCount = mesh.subMeshCount;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] tris = mesh.GetTriangles(i);
            if (tris != null && tris.Length > 0)
                newMesh.SetTriangles(tris, i);
        }

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        if (overwriteOriginal)
        {
            string tempPath = assetPath.Replace(".asset", "_temp.asset");
            AssetDatabase.CreateAsset(newMesh, tempPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.DeleteAsset(assetPath);
            File.Move(tempPath, assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Mesh overwritten: " + assetPath);
        }
        else
        {
            int lastSlash = assetPath.LastIndexOf('/');
            string folder = (lastSlash >= 0) ? assetPath.Substring(0, lastSlash) : "Assets";
            string newPath = folder + "/" + newMesh.name + ".asset";
            AssetDatabase.CreateAsset(newMesh, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Mesh saved to: " + newPath);
        }
    }
}
