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

        string[] guids = AssetDatabase.FindAssets("t:Material", new string[] { SEARCH_PATH });
        foreach (string guid in guids)
        {
            string fullPath = AssetDatabase.GUIDToAssetPath(guid);

            MaterialInfo info;
            info.path = Path.ChangeExtension(fullPath.Substring(SEARCH_PATH.Length + 1), null);
            info.name = Path.GetFileName(info.path);
            if (info.path.Length <= info.name.Length)
                info.category = "";
            else
                info.category = Path.GetFileName(
                    info.path.Substring(0, info.path.Length - info.name.Length - 1));
            if (info.path.Contains("Materials"))
                info.layer = PaintLayer.MATERIAL;
            else if (info.path.Contains("Overlays"))
                info.layer = PaintLayer.OVERLAY;
            else if (info.path.Contains("Skies"))
                info.layer = PaintLayer.SKY;
            else
                info.layer = PaintLayer.HIDDEN;
            if (info.category == "Materials" || info.category == "Overlays" || info.category == "Skies")
                info.category = "";

            database.materials.Add(info);
        }

        AssetDatabase.CreateAsset(database, "Assets/Resources/materials.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = database;

        Resources.UnloadUnusedAssets();
        Debug.Log("done!");
    }

    private static MaterialInfo? SearchDatabase(MaterialDatabase database, string name)
    {
        foreach (MaterialInfo info in database.materials)
        {
            if (info.name == name)
                return info;
        }
        return null;
    }
}