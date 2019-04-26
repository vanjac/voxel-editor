using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldFiles
{
    public static string GetWorldsDirectory()
    {
        return Application.persistentDataPath;
    }

    public static string GetNewWorldPath(string name)
    {
        return GetWorldsDirectory() + "/" + name + ".nspace";
    }

    public static bool IsWorldFile(string path)
    {
        return path.ToLower().EndsWith(".nspace");
    }

    public static bool IsOldWorldFile(string path)
    {
        return path.ToLower().EndsWith(".json");
    }
}