using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldFiles
{
    public static string GetDirectoryPath()
    {
        return Application.persistentDataPath;
    }

    public static string GetFilePath(string name)
    {
        return GetDirectoryPath() + "/" + name + ".json";
    }
}