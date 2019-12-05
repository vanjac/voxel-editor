using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SelectedWorld
{
    private static string worldPath = WorldFiles.GetNewWorldPath("mapsave");
    private static TextAsset demoWorldAsset = null;

    public static void SelectSavedWorld(string path)
    {
        worldPath = path;
        demoWorldAsset = null;
    }

    public static void SelectDemoWorld(TextAsset asset, string savePath)
    {
        worldPath = savePath;
        demoWorldAsset = asset;
    }

    public static Stream GetLoadStream()
    {
        if (demoWorldAsset == null)
            return File.Open(worldPath, FileMode.Open);
        else
            return new MemoryStream(demoWorldAsset.bytes);
    }

    public static string GetSavePath()
    {
        return worldPath;
    }
}
