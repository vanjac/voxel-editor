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
}
