using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class UpdateMaterialDatabase
{
    private const string SEARCH_PATH = "Assets/Resources/GameAssets";

    [MenuItem("Tools/Update N-Space material database")]
    public static void UpdateMaterials()
    {
        MaterialDatabase database = ScriptableObject.CreateInstance<MaterialDatabase>();

        string[] guids = AssetDatabase.FindAssets("", new string[] { SEARCH_PATH });
        foreach (string guid in guids)
        {
            MaterialInfo info;
            string fullPath = AssetDatabase.GUIDToAssetPath(guid);
            info.path = Path.ChangeExtension(fullPath.Substring(SEARCH_PATH.Length + 1), null);
            info.name = Path.GetFileName(info.path);
            if (info.path.Length <= info.name.Length)
                info.parent = "";
            else
                info.parent = info.path.Substring(0, info.path.Length - info.name.Length - 1);
            info.isDirectory = Path.GetExtension(fullPath) == "";
            database.materials.Add(info);
        }

        AssetDatabase.CreateAsset(database, "Assets/Resources/materials.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = database;
        Debug.Log("done!");
    }
}