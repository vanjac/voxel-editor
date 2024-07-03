using System.IO;
using UnityEngine;

public static class SelectedWorld {
    private static string worldPath = WorldFiles.GetNewWorldPath("mapsave");
    private static TextAsset demoWorldAsset = null;

    public static void SelectSavedWorld(string path) {
        worldPath = path;
        demoWorldAsset = null;
    }

    public static void SelectDemoWorld(TextAsset asset, string savePath) {
        worldPath = savePath;
        demoWorldAsset = asset;
    }

    // this creates a stream, so make sure to wrap it in a "using" block!
    public static Stream GetLoadStream() {
        try {
            if (demoWorldAsset == null) {
                return File.Open(worldPath, FileMode.Open);
            } else {
                return new MemoryStream(demoWorldAsset.bytes);
            }
        } catch (System.Exception e) {
            throw new MapReadException("Error opening file", e);
        }
    }

    public static string GetSavePath() {
        demoWorldAsset = null; // load from the saved file next time
        return worldPath;
    }
}
