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

    public static Material MakeCustomMaterial(Shader shader)
    {
        Material material = new Material(shader);
        material.name = "Custom" + System.Guid.NewGuid();
        return material;
    }

    public static bool IsCustomMaterial(Material material)
    {
        return material.name.StartsWith("Custom");
    }
}
