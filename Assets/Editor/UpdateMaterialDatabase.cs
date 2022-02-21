using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class UpdateMaterialDatabase
{
    private const string SEARCH_PATH = "Assets/Resources/GameAssets";
    private const string SOUND_LABEL_PREFIX = "Sound_";

    [MenuItem("Tools/Update N-Space material database")]
    public static void UpdateMaterials()
    {
        MaterialDatabase database = ScriptableObject.CreateInstance<MaterialDatabase>();

        string[] guids = AssetDatabase.FindAssets("", new string[] { SEARCH_PATH });
        foreach (string guid in guids)
        {
            string fullPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(fullPath);

            MaterialInfo info;
            info.path = Path.ChangeExtension(fullPath.Substring(SEARCH_PATH.Length + 1), null);
            info.name = Path.GetFileName(info.path);
            if (info.path.Length <= info.name.Length)
                info.parent = "";
            else
                info.parent = info.path.Substring(0, info.path.Length - info.name.Length - 1);
            info.isDirectory = material == null;
            info.sound = MaterialSound.GENERIC;
            if (material != null)
            {
                foreach (string label in AssetDatabase.GetLabels(material))
                {
                    if (label.StartsWith(SOUND_LABEL_PREFIX))
                    {
                        string soundName = label.Replace(SOUND_LABEL_PREFIX, "").ToUpper();
                        info.sound = (MaterialSound)System.Enum.Parse(typeof(MaterialSound), soundName);
                    }
                }
            }

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