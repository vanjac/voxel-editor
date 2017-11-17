using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesDirectory : MonoBehaviour {
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
}
