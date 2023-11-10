// https://answers.unity.com/answers/746039/view.html
using UnityEngine;
using UnityEditor;
using System.Linq;

public class CreateMaterialsForTextures : ScriptableWizard
{
    public Shader shader;

    [MenuItem("Tools/CreateMaterialsForTextures")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<CreateMaterialsForTextures>("Create Materials", "Create");
    }

    void OnEnable()
    {
        shader = Shader.Find("Standard");
    }

    void OnWizardCreate()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            var textures = Selection.GetFiltered(typeof(Texture), SelectionMode.Assets).Cast<Texture>();
            foreach (var tex in textures)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                path = path.Substring(0, path.LastIndexOf(".")) + ".mat";
                if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null)
                {
                    Debug.LogWarning("Can't create material, it already exists: " + path);
                    continue;
                }
                var mat = new Material(shader);
                mat.mainTexture = tex;
                AssetDatabase.CreateAsset(mat, path);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
    }
}