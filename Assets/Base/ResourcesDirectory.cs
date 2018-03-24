using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourcesDirectory
{
    private static string[] _dirList = null;
    public static string[] dirList
    {
        get
        {
            if (_dirList == null)
            {
                TextAsset dirListText = Resources.Load<TextAsset>("dirlist");
                _dirList = dirListText.text.Split('\n');
                // fix issue when checking out a branch in git:
                for (int i = 0; i < _dirList.Length; i++)
                    _dirList[i] = _dirList[i].Trim();
                Resources.UnloadAsset(dirListText);
            }
            return _dirList;
        }
    }

    public static Material GetMaterial(string path)
    {
        // remove extension if necessary
        path = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);
        return Resources.Load<Material>(path);
    }

    public static Material MakeCustomMaterial(Shader shader, bool transparent=false)
    {
        Material material = new Material(shader);
        material.name = "Custom" + System.Guid.NewGuid();
        if (transparent)
        {
            // http://answers.unity.com/answers/1265884/view.html
            // only applies to standard shader!
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
        if (shader.name == "Standard")
            material.SetFloat("_Glossiness", 0.2f);
        return material;
    }

    public static bool IsCustomMaterial(Material material)
    {
        return material.name.StartsWith("Custom");
    }
}
