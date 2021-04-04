using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WorldFiles
{
    public static string GetWorldsDirectory()
    {
        return Application.persistentDataPath;
    }

    public static string GetNewWorldPath(string name)
    {
        return Path.Combine(GetWorldsDirectory(), name + ".nspace");
    }

    public static bool IsWorldFile(string path)
    {
        return path.ToLower().EndsWith(".nspace");
    }

    public static bool IsOldWorldFile(string path)
    {
        return path.ToLower().EndsWith(".json");
    }

    public static void ListWorlds(List<string> worldPaths, List<string> worldNames)
    {
        string[] files = Directory.GetFiles(WorldFiles.GetWorldsDirectory());
        worldPaths.Clear();
        foreach (string path in files)
        {
            if (WorldFiles.IsWorldFile(path))
                worldPaths.Add(path);
            else if (WorldFiles.IsOldWorldFile(path))
            {
                string newPath = WorldFiles.GetNewWorldPath(Path.GetFileNameWithoutExtension(path));
                Debug.Log("Updating " + path + " to " + newPath);
                File.Move(path, newPath);
                worldPaths.Add(newPath);
            }
        }
        worldPaths.Sort();

        worldNames.Clear();
        foreach (string path in worldPaths)
            worldNames.Add(Path.GetFileNameWithoutExtension(path));
    }

    public static bool ValidateName(string name, out string errorMessage)
    {
        errorMessage = null;
        if (name.Length == 0)
            return false;
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
        {
            errorMessage = "That name contains a special character which is not allowed.";
            return false;
}

        if (name.StartsWith("."))
        {
            errorMessage = "Name can't start with a period.";
            return false;
        }

        string path = WorldFiles.GetNewWorldPath(name);
        if (File.Exists(path))
        {
            errorMessage = "A world with that name already exists.";
            return false;
        }
        return true;  // you are valid <3
    }
}
